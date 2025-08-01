using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.Events
{
    public class EventBus
    {
        // 使用强引用委托列表，而不是WeakReference，以确保订阅的稳定性和可靠性。
        // 在tModLoader环境中，系统和NPC的生命周期是可控的，不需要担心EventBus导致的内存泄漏。
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
        private readonly object _lock = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<Delegate>();
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
            var eventType = e.GetType(); // 使用 e.GetType() 而不是 typeof(T) 来支持发布派生类事件
            List<Delegate> handlersSnapshot;

            lock (_lock)
            {
                if (!_subscribers.TryGetValue(eventType, out var handlers))
                    return;

                // 创建一个快照以在锁外执行，防止死锁
                handlersSnapshot = new List<Delegate>(handlers);
            }

            // 在锁外执行委托，避免长时间持有锁
            foreach (var handler in handlersSnapshot)
            {
                // 检查委托是否仍然有效且类型匹配
                if (handler is Action<T> typedHandler)
                {
                    try
                    {
                        typedHandler(e);
                    }
                    catch (Exception ex)
                    {
                        // 记录异常，而不是让一个订阅者的错误中断整个事件分发
                        Terraria.ModLoader.ModContent.GetInstance<AshenVoid>().Logger.Error($"Error executing event handler for {eventType.Name}", ex);
                    }
                }
            }
        }
    }
}