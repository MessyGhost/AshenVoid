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
        // The queue now stores serialized event data.
        private readonly Queue<byte[]> _packetQueue = new();

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
        /// Serializes a network event and queues it to be sent.
        /// The original event object is not stored and can be safely released.
        /// </summary>
        public void Send(INetworkEvent e)
        {
            if (!_eventTypeToId.TryGetValue(e.GetType(), out var id))
                return;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.SyncEvent);
            writer.Write(id);
            e.Write(writer);

            _packetQueue.Enqueue(ms.ToArray());
        }

        public void SendPendingMessages()
        {
            if (_packetQueue.Count == 0)
                return;

            var packet = ModContent.GetInstance<AshenVoid>().GetPacket();

            while (_packetQueue.Count > 0)
            {
                var data = _packetQueue.Dequeue();
                packet.Write(data);
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
                        _eventBus.Publish(newEvent);
                    }
                    break;
            }
        }
    }
}