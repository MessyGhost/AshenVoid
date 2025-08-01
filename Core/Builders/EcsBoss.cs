using AshenVoid.Core.ECS;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    // This class is the entry point for the ECS logic for a given NPC.
    // It should be as lean as possible, delegating all logic to Systems.
    public abstract class EcsBoss : ModNPC
    {
        public ComponentController Controller { get; private set; }
        protected EventBus EventBus { get; private set; }

        // This method is where you define all the components and systems for this boss.
        protected abstract ComponentController InitializeController();

        public sealed override void SetDefaults()
        {
            EventBus = new EventBus();
            Controller = InitializeController();

            // Call the abstract method for ModNPC specific defaults
            SetBossDefaults();

            NPC.aiStyle = -1;
            NPC.netAlways = true; // Important for custom AI

            // The system cache should be built once all components and systems are registered.
            Controller.BuildSystemCache();
        }

        // Implement this in your derived class to set NPC properties.
        public abstract void SetBossDefaults();

        public override void OnSpawn(IEntitySource source)
        {
            // OnSpawn logic, if any, should be handled by a dedicated System.
            // The initial state is now set within InitializeController.
        }

        // The AI method is now extremely simple. It just ticks the ECS engine.
        public override void AI()
        {
            Controller?.Update(Main.gameTimeCache, NPC, EventBus);
        }

        // Network synchronization is now handled by two mechanisms:
        // 1. Event-based ModPackets for immediate state changes (handled in StateMachine and Mod class).
        // 2. SendExtraAI/ReceiveExtraAI for continuous data sync (like health, position).
        public override void SendExtraAI(BinaryWriter writer)
        {
            // We find components that need syncing and ask them to write their data.
            var networkedComponents = Controller?.GetNetworkedComponents();
            if (networkedComponents != null)
            {
                foreach (var component in networkedComponents)
                {
                    component.SendData(NPC, writer);
                }
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            var networkedComponents = Controller?.GetNetworkedComponents();
            if (networkedComponents != null)
            {
                foreach (var component in networkedComponents)
                {
                    component.ReceiveData(NPC, reader);
                }
            }
        }

        // Event publishing for game events.
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                EventBus?.Publish(new NPCDamagedEvent(NPC, hit));
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                EventBus?.Publish(new NPCDamagedEvent(NPC, hit));
        }
    }
}