using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS.Systems
{
    public class NetworkEventSystem : ISystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;
        public IEnumerable<System.Type> RequiredComponents { get; } = new List<System.Type>();

        private readonly EventBus _eventBus;
        private readonly List<IEvent> _eventQueue = new List<IEvent>();

        public NetworkEventSystem(EventBus eventBus)
        {
            _eventBus = eventBus;
            // Subscribe to all network events. A more optimized approach might use specific subscriptions.
            _eventBus.Subscribe<INetworkEvent>(OnNetworkEvent);
        }

        private void OnNetworkEvent(INetworkEvent e)
        {
            // Queue events to be sent in the update loop.
            _eventQueue.Add(e);
        }

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            if (Main.netMode != NetmodeID.Server) return;

            foreach (var e in _eventQueue)
            {
                if (e is INetworkEvent networkEvent)
                {
                    var packet = ModContent.GetInstance<AshenVoid>().GetPacket();
                    packet.Write((byte)AshenVoid.MessageType.SyncNetworkEvent);
                    
                    // Write NPC id to associate the event with the correct boss
                    packet.Write((byte)npc.whoAmI);

                    // Write the event type so the client knows how to deserialize it.
                    packet.Write(e.GetType().AssemblyQualifiedName);
                    
                    // Write the event data.
                    networkEvent.Write(packet);
                    
                    packet.Send();
                }
            }
            _eventQueue.Clear();
        }
    }
}