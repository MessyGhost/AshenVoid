using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.Events
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<WeakReference<Delegate>>> _subscribers = new();
        private readonly Dictionary<Type, object> _liveHandlerPool = new();
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
                        wr.TryGetTarget(out var existingHandler) && existingHandler.Equals(handler));

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
            List<Action<T>> liveHandlers;

            lock (_lock)
            {
                if (!_subscribers.TryGetValue(eventType, out var subscribers))
                    return;

                if (!_liveHandlerPool.TryGetValue(eventType, out var pool))
                {
                    pool = new List<Action<T>>();
                    _liveHandlerPool[eventType] = pool;
                }
                liveHandlers = (List<Action<T>>)pool;
                liveHandlers.Clear();

                // Using a reverse loop is safer for removal
                for (int i = subscribers.Count - 1; i >= 0; i--)
                {
                    var weakHandler = subscribers[i];
                    if (weakHandler.TryGetTarget(out var handlerDelegate))
                    {
                        if (handlerDelegate is Action<T> handler)
                        {
                            liveHandlers.Add(handler);
                        }
                    }
                    else
                    {
                        // Remove dead reference
                        subscribers.RemoveAt(i);
                    }
                }
            }

            // Execute handlers outside the lock to prevent deadlocks
            foreach (var handler in liveHandlers)
            {
                handler(e);
            }
        }
    }
}