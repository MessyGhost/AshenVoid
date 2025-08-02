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

        /// <summary>
        /// Publishes an event. The lifecycle of the event is managed by the caller.
        /// The EventBus will NOT create or release the event.
        /// </summary>
        public void Publish(IEvent e)
        {
            lock (_lock)
            {
                _eventQueue.Enqueue(e);
            }
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
                        continue;
                    }
                    _isDispatching = true;
                }

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

                lock (_lock)
                {
                    _isDispatching = false;
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