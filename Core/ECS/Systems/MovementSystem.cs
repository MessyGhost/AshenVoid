using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class MovementSystem : EntityQuerySystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(MovementComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        protected override void UpdateArchetype(GameTime gameTime, Archetype archetype, EcsWorld world, EventBus eventBus)
        {
            var movements = archetype.GetComponentSpan<MovementComponent>();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < movements.Length; i++)
            {
                var movement = movements[i];
                movement.Dynamics.Update(deltaTime, movement.TargetPosition);
                movement.Npc.Center = movement.Dynamics.Position;
            }
        }
    }
}