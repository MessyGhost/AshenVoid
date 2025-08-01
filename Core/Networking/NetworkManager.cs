using AshenVoid.Core.Events;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;

namespace AshenVoid.Core.Networking
{
    public enum MessageType : byte
    {
        SyncEvent,
    }

    public class NetworkManager
    {
        private readonly EventBus _eventBus;
        private readonly Queue<INetworkEvent> _eventQueue = new();

        private readonly Dictionary<Type, byte> _eventTypeToId = new();
        private readonly Dictionary<byte, Func<INetworkEvent>> _idToEventFactory = new();
        private byte _nextEventId = 0;

        public NetworkManager(EventBus eventBus)
        {
            _eventBus = eventBus;
            // The automatic subscription is removed to decouple event publishing from network sending.
            // _eventBus.Subscribe<INetworkEvent>(QueueEvent);
        }

        public void RegisterEventType<T>() where T : INetworkEvent, new()
        {
            var type = typeof(T);
            var id = _nextEventId++;
            _eventTypeToId[type] = id;
            _idToEventFactory[id] = () => new T(); // Register the factory function
        }

        /// <summary>
        /// Explicitly queues a network event to be sent to clients/server.
        /// </summary>
        public void Send(INetworkEvent e)
        {
            _eventQueue.Enqueue(e);
        }

        public void SendPendingMessages()
        {
            if (_eventQueue.Count == 0)
                return;

            var packet = ModContent.GetInstance<AshenVoid>().GetPacket();

            while (_eventQueue.Count > 0)
            {
                var e = _eventQueue.Dequeue();
                if (_eventTypeToId.TryGetValue(e.GetType(), out var id))
                {
                    packet.Write((byte)MessageType.SyncEvent);
                    packet.Write(id);
                    e.Write(packet);
                }
            }

            packet.Send();
        }

        public void HandlePacket(BinaryReader reader, int whoAmI)
        {
            var msgType = (MessageType)reader.ReadByte();
            switch (msgType)
            {
                case MessageType.SyncEvent:
                    var eventId = reader.ReadByte();
                    if (_idToEventFactory.TryGetValue(eventId, out var factory))
                    {
                        var e = factory(); // Use the factory, no reflection
                        e.Read(reader);
                        _eventBus.Publish(e);
                    }
                    break;
            }
        }
    }
}