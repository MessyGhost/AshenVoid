using System;
using AshenVoid.Core.Utility;
using System.Collections.Generic;

namespace AshenVoid.Core.Events
{
    public class EventBus
    {
        // 非泛型接口，用于统一调用
        private interface IEventHandlerWrapper
        {
            void Invoke(IEvent e);
            bool IsHandler(object handler);
        }

        // 泛型包装器，持有强类型委托
        private class EventHandlerWrapper<T> : IEventHandlerWrapper where T : IEvent
        {
            private readonly Action<T> _handler;

            public EventHandlerWrapper(Action<T> handler)
            {
                _handler = handler;
            }

            public void Invoke(IEvent e)
            {
                _handler((T)e);
            }

            public bool IsHandler(object handler)
            {
                return _handler.Equals(handler);
            }
        }

        private readonly Dictionary<Type, List<IEventHandlerWrapper>> _subscribers = new();
        private readonly Queue<IEvent> _eventQueue = new();
        private readonly object _lock = new();

        private bool _isDispatching = false;
        private readonly List<Action> _pendingModifications = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            Action addAction = () =>
            {
                var eventType = typeof(T);
                if (!_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<IEventHandlerWrapper>();
                    _subscribers[eventType] = handlers;
                }
                handlers.Add(new EventHandlerWrapper<T>(handler));
            };

            lock (_lock)
            {
                if (_isDispatching)
                {
                    _pendingModifications.Add(addAction);
                }
                else
                {
                    addAction();
                }
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            Action removeAction = () =>
            {
                var eventType = typeof(T);
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers.RemoveAll(wrapper => wrapper.IsHandler(handler));
                }
            };

            lock (_lock)
            {
                if (_isDispatching)
                {
                    _pendingModifications.Add(removeAction);
                }
                else
                {
                    removeAction();
                }
            }
        }

        public void Publish<T>(T e) where T : IEvent
        {
            lock (_lock)
            {
                _eventQueue.Enqueue(e);
            }
        }

        /// <summary>
        /// Gets an event from the object pool, initializes it, and publishes it.
        /// This is the preferred way to publish events to avoid GC allocation.
        /// </summary>
        public void Publish<T>(Action<T> initializer) where T : class, IEvent, new()
        {
            var e = ObjectPool.Get<T>();
            initializer(e);
            Publish(e); // Enqueue it using the existing method
        }

        public void DispatchEvents()
        {
            Queue<IEvent> queueSnapshot;
            lock (_lock)
            {
                if (_eventQueue.Count == 0)
                    return;

                queueSnapshot = new Queue<IEvent>(_eventQueue);
                _eventQueue.Clear();
            }

            while (queueSnapshot.Count > 0)
            {
                var e = queueSnapshot.Dequeue();
                var eventType = e.GetType();

                List<IEventHandlerWrapper> handlers;
                lock (_lock)
                {
                    if (!_subscribers.TryGetValue(eventType, out handlers))
                    {
                        // If no one is listening, just release the event back to the pool
                        if (e is not null && e.GetType().GetConstructor(Type.EmptyTypes) != null)
                        {
                            ObjectPool.Release(e);
                        }
                        continue;
                    }
                    _isDispatching = true;
                }

                // No snapshotting here, iterate over the original list
                foreach (var handlerWrapper in handlers)
                {
                    try
                    {
                        handlerWrapper.Invoke(e);
                    }
                    catch (Exception ex)
                    {
                        Terraria.ModLoader.ModContent.GetInstance<AshenVoid>().Logger.Error($"Error executing event handler for {eventType.Name}", ex);
                    }
                }

                // Release the event back to the pool after all handlers have processed it.
                // We check if it's a reference type and has a parameterless constructor,
                // which are the constraints for our pooling system.
                if (e is not null && e.GetType().GetConstructor(Type.EmptyTypes) != null)
                {
                    ObjectPool.Release(e);
                }


                lock (_lock)
                {
                    _isDispatching = false;
                    // Process any modifications that were queued during dispatch
                    foreach (var modification in _pendingModifications)
                    {
                        modification();
                    }
                    _pendingModifications.Clear();
                }
            }
        }
    }
}