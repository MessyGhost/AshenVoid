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
                    health.CurrentHealth = e.NPC.life;
                }
            }
        }

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var health = world.GetComponent<HealthComponent>(entityId);
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);

            if (health != null && statSheet != null)
            {
                // Sync ECS health to NPC health. The event is for reacting to damage.
                health.CurrentHealth = statSheet.Npc.life;
            }
        }
    }
}