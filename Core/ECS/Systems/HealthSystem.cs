using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class HealthSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(HealthComponent), typeof(StatSheetComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public HealthSystem()
        {
            EcsSystem.Instance.EventBus.Subscribe<NPCDamagedEvent>(OnNpcDamaged);
        }

        private void OnNpcDamaged(NPCDamagedEvent e)
        {
            if (EcsSystem.NpcWhoAmIToEntityId.TryGetValue(e.NPC.whoAmI, out int entityId))
            {
                var world = EcsSystem.Instance.World;
                var health = world.GetComponent<HealthComponent>(entityId);

                if (health != null)
                {
                    // The single source of truth for health updates is now this event.
                    health.CurrentHealth = e.NPC.life;
                }
            }
        }

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            // The polling logic has been removed.
            // All health synchronization is now handled by the OnNpcDamaged event.
        }
    }
}