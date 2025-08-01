using AshenVoid.Core.Abilities;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.Systems
{
    public class AbilitySystem : EntityQuerySystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(AbilityComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var abilityComponent = world.GetComponent<AbilityComponent>(entityId);
            if (abilityComponent?.ActiveAbility == null) return;

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            abilityComponent.ActiveAbility.Update(deltaTime, entityId, world);

            // If the ability finishes, it should handle its own state transition to Cooldown
            // and the AbilityComponent should clear the ActiveAbility.
            // For now, we assume the ability itself will manage its lifecycle.
        }
    }
}