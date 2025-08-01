using AshenVoid.Core.ECS;
using AshenVoid.Core.Events;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    /// <summary>
    /// Base class for a boss that is managed by the EcsSystem.
    /// It acts as a bridge between a Terraria ModNPC and an ECS entity.
    /// </summary>
    public abstract class EcsBoss : ModNPC
    {
        /// <summary>
        /// The unique identifier for this NPC in the EcsWorld.
        /// It is -1 until synchronized from the server.
        /// </summary>
        public int EntityId { get; private set; } = -1;

        /// <summary>
        /// Defines the components and systems for this boss entity.
        /// This is where you add components and systems to the EcsWorld.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="entityId">The entity ID for this boss.</param>
        protected abstract void BuildEntity(EcsWorld world, int entityId);

        public sealed override void SetDefaults()
        {
            SetBossDefaults();
            NPC.aiStyle = -1;
            NPC.netAlways = true;
        }

        public abstract void SetBossDefaults();

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                return;

            var world = EcsSystem.Instance.World;
            EntityId = world.CreateEntity();

            // Add to the global mapping
            EcsSystem.NpcWhoAmIToEntityId[NPC.whoAmI] = EntityId;

            BuildEntity(world, EntityId);

            // Publish the sync event to all clients
            var syncEvent = new EntityIdSyncEvent(NPC.whoAmI, EntityId);
            EcsSystem.Instance.EventBus.Publish(syncEvent);
        }

        public override void OnKill()
        {
            if (EntityId != -1)
            {
                // Remove from the global mapping
                if (EcsSystem.NpcWhoAmIToEntityId.ContainsKey(NPC.whoAmI))
                {
                    EcsSystem.NpcWhoAmIToEntityId.Remove(NPC.whoAmI);
                }

                EcsSystem.Instance.World.DestroyEntity(EntityId);
                EntityId = -1;
            }
        }

        public override void Load()
        {
            EcsSystem.Instance.EventBus.Subscribe<EntityIdSyncEvent>(HandleEntityIdSync);
        }

        public override void Unload()
        {
            EcsSystem.Instance.EventBus.Unsubscribe<EntityIdSyncEvent>(HandleEntityIdSync);
        }

        private void HandleEntityIdSync(EntityIdSyncEvent e)
        {
            // On clients, if the NPC ID matches, set the EntityId
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient && e.NpcWhoAmI == NPC.whoAmI)
            {
                EntityId = e.EntityId;
                // Also add to the client-side mapping
                EcsSystem.NpcWhoAmIToEntityId[NPC.whoAmI] = e.EntityId;
            }
        }

        // The AI, SendExtraAI, and ReceiveExtraAI methods are now obsolete.
        // All logic will be handled by Systems in the EcsSystem.
        public sealed override void AI() { }
        public sealed override void SendExtraAI(System.IO.BinaryWriter writer) { }
        public sealed override void ReceiveExtraAI(System.IO.BinaryReader reader) { }
    }
}