using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class HealthSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(HealthComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public HealthSystem()
        {
            // Subscribe to damage events
            EcsSystem.Instance.EventBus.Subscribe<NPCDamagedEvent>(OnNpcDamaged);
        }

        private void OnNpcDamaged(NPCDamagedEvent e)
        {
            // This is a naive implementation. It doesn't correctly map the NPC to the entity.
            // This will be fixed with a proper entity mapping system.
        }

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var health = world.GetComponent<HealthComponent>(entityId);
            // Health update logic, e.g., checking for death
        }
    }
}