using System;
using System.Collections.Generic;

namespace AshenVoid.Core.Events
{
    public class EventBus
    {
        // 非泛型接口，用于统一调用
        private interface IEventHandlerWrapper
        {
            void Invoke(IEvent e);
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
                // 直接调用，类型在订阅时已确定
                _handler((T)e);
            }

            // 用于在 Unsubscribe 中比较
            public override bool Equals(object obj)
            {
                return obj is EventHandlerWrapper<T> other && _handler.Equals(other._handler);
            }

            public override int GetHashCode()
            {
                return _handler.GetHashCode();
            }
        }

        private readonly Dictionary<Type, List<IEventHandlerWrapper>> _subscribers = new();
        private readonly Queue<IEvent> _eventQueue = new();
        private readonly object _lock = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            var wrapper = new EventHandlerWrapper<T>(handler);

            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<IEventHandlerWrapper>();
                }
                _subscribers[eventType].Add(wrapper);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            var wrapper = new EventHandlerWrapper<T>(handler);

            lock (_lock)
            {
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(wrapper);
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
                List<IEventHandlerWrapper> handlersSnapshot;

                lock (_lock)
                {
                    if (!_subscribers.TryGetValue(eventType, out var handlers))
                        continue;

                    handlersSnapshot = new List<IEventHandlerWrapper>(handlers);
                }

                foreach (var handlerWrapper in handlersSnapshot)
                {
                    try
                    {
                        // 无反射，直接调用
                        handlerWrapper.Invoke(e);
                    }
                    catch (Exception ex)
                    {
                        Terraria.ModLoader.ModContent.GetInstance<AshenVoid>().Logger.Error($"Error executing event handler for {eventType.Name}", ex);
                    }
                }
            }
        }
    }
}