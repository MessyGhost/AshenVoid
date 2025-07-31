using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.Events
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<WeakReference<Delegate>>> _subscribers = new();
        private readonly object _lock = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            var weakHandler = new WeakReference<Delegate>(handler);

            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<WeakReference<Delegate>>();
                }
                _subscribers[eventType].Add(weakHandler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            lock (_lock)
            {
                if (_subscribers.TryGetValue(eventType, out var subscribers))
                {
                    var weakRefToRemove = subscribers.FirstOrDefault(wr =>
                    {
                        return wr.TryGetTarget(out var existingHandler) && existingHandler.Equals(handler);
                    });

                    if (weakRefToRemove != null)
                    {
                        subscribers.Remove(weakRefToRemove);
                    }
                }
            }
        }

        public void Publish<T>(T e) where T : IEvent
        {
            var eventType = typeof(T);
            var liveHandlers = new List<Action<T>>();
            var deadHandlers = new List<WeakReference<Delegate>>();

            lock (_lock)
            {
                if (!_subscribers.TryGetValue(eventType, out var subscribers))
                    return;

                foreach (var weakHandler in subscribers)
                {
                    if (weakHandler.TryGetTarget(out var handlerDelegate))
                    {
                        if (handlerDelegate is Action<T> handler)
                        {
                            liveHandlers.Add(handler);
                        }
                    }
                    else
                    {
                        deadHandlers.Add(weakHandler);
                    }
                }

                if (deadHandlers.Any())
                {
                    foreach (var dead in deadHandlers)
                    {
                        subscribers.Remove(dead);
                    }
                }
            }

            foreach (var handler in liveHandlers)
            {
                handler(e);
            }
        }
    }
}