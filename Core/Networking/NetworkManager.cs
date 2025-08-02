using AshenVoid.Core.Events;
using AshenVoid.Core.Utility;
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
        }

        public void RegisterEventType<T>() where T : INetworkEvent, new()
        {
            var type = typeof(T);
            var id = _nextEventId++;
            _eventTypeToId[type] = id;
            _idToEventFactory[id] = () => new T();
        }

        /// <summary>
        /// Queues a network event to be sent.
        /// The NetworkManager takes ownership of the event and will release it after sending.
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

                // Release the event back to the pool now that it has been serialized.
                if (e is not null && e.GetType().GetConstructor(Type.EmptyTypes) != null)
                {
                    ObjectPool.Release(e);
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
                        var newEvent = factory();
                        newEvent.Read(reader);
                        // The event received from the network is new, so it's not owned by anyone yet.
                        // We publish it, but who releases it?
                        // Let's assume for now that network-received events are not pooled and will be GC'd.
                        // A more advanced system could pool them on the client too.
                        _eventBus.Publish(newEvent);
                    }
                    break;
            }
        }
    }
}