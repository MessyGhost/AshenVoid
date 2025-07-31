using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.Events
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }
            _subscribers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Remove(handler);
            }
        }

        public void Publish<T>(T e) where T : IEvent
        {
            var eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                // Create a copy to prevent issues if a handler unsubscribes during iteration
                var handlers = _subscribers[eventType].ToList();
                foreach (var handler in handlers)
                {
                    ((Action<T>)handler)(e);
                }
            }
        }
    }
}