using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AttackSystem))]
    public class HealthSystem : IHealthSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public HashSet<Type> RequiredComponents { get; } = new HashSet<Type>
        {
            typeof(HealthComponent)
        };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            var health = controller.GetComponent<HealthComponent>();

            // This system could be used to monitor health changes,
            // apply regeneration, or publish events on certain health thresholds.
            // The current damage event publishing is handled in EcsBoss.cs, which is fine for now.
        }
    }
}