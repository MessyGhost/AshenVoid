using AshenVoid.Core.ECS;
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
            // Set ModNPC specific defaults first.
            SetBossDefaults();

            NPC.aiStyle = -1;
            NPC.netAlways = true;
        }

        public abstract void SetBossDefaults();

        public override void OnSpawn(IEntitySource source)
        {
            // Do not create the entity on the client if it's a multiplayer game.
            // The entity will be created and synced from the server.
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                return;

            // Create an entity in the global ECS world.
            var world = EcsSystem.Instance.World;
            EntityId = world.CreateEntity();

            // Build the entity with its components and systems.
            BuildEntity(world, EntityId);
            
            // TODO: Need a mechanism to sync the EntityId to clients.
        }
        
        public override void OnKill()
        {
            // TODO: Need a way to destroy the entity in the EcsWorld.
        }

        // The AI, SendExtraAI, and ReceiveExtraAI methods are now obsolete.
        // All logic will be handled by Systems in the EcsSystem.
        public sealed override void AI() { }
        public sealed override void SendExtraAI(System.IO.BinaryWriter writer) { }
        public sealed override void ReceiveExtraAI(System.IO.BinaryReader reader) { }
    }
}