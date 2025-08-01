using System;
using System.Collections.Generic;

namespace AshenVoid.Core.Events
{
    public class EventBus
    {
        private readonly Dictionary<Type, HashSet<Delegate>> _subscribers = new();
        private readonly Queue<IEvent> _eventQueue = new();
        private readonly object _lock = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new HashSet<Delegate>();
                }
                _subscribers[eventType].Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            lock (_lock)
            {
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);
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
                HashSet<Delegate> handlersSnapshot;

                lock (_lock)
                {
                    if (!_subscribers.TryGetValue(eventType, out var handlers))
                        continue;

                    handlersSnapshot = new HashSet<Delegate>(handlers);
                }

                foreach (var handler in handlersSnapshot)
                {
                    try
                    {
                        // Instead of 'is', we can use direct invocation after ensuring the delegate type.
                        // This is slightly faster as the type is known from subscription.
                        handler.DynamicInvoke(e);
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