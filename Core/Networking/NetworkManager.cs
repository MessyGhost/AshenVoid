using AshenVoid.Core.Events;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;

namespace AshenVoid.Core.Networking
{
    public enum MessageType : byte
    {
        SyncEvent,
        // Other message types like SyncComponent, SpawnEntity etc. will be added later
    }

    public class NetworkManager
    {
        private readonly EventBus _eventBus;
        private readonly Queue<INetworkEvent> _eventQueue = new Queue<INetworkEvent>();

        public NetworkManager(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<INetworkEvent>(QueueEvent);
        }

        private void QueueEvent(INetworkEvent e)
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
                packet.Write((byte)MessageType.SyncEvent);
                // We need a way to map event types to a byte ID instead of using strings.
                packet.Write(e.GetType().AssemblyQualifiedName); 
                e.Write(packet);
            }

            packet.Send();
        }

        public void HandlePacket(BinaryReader reader, int whoAmI)
        {
            var msgType = (MessageType)reader.ReadByte();
            switch (msgType)
            {
                case MessageType.SyncEvent:
                    // Deserialize and handle the event
                    var typeName = reader.ReadString();
                    var type = System.Type.GetType(typeName);
                    if (type != null && typeof(INetworkEvent).IsAssignableFrom(type))
                    {
                        var e = (INetworkEvent)System.Activator.CreateInstance(type);
                        e.Read(reader);
                        _eventBus.Publish(e); // Publish to the client-side event bus
                    }
                    break;
            }
        }
    }
}