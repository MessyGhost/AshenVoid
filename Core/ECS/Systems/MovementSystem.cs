using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class MovementSystem : CachedComponentSystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(MovementComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var movement = world.GetComponent<MovementComponent>(entityId);
            if (movement == null) return;

            // The logic from MovementComponent.Update is now here.
            movement.Dynamics.Update((float)gameTime.ElapsedGameTime.TotalSeconds, movement.TargetPosition);
            movement.Npc.Center = movement.Dynamics.Position;
        }
    }
}