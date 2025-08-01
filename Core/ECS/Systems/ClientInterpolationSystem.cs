using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class ClientInterpolationSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(MovementComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var movement = world.GetComponent<MovementComponent>(entityId);
            if (movement == null) return;
            
            // The logic from MovementComponent.Interpolate is now here.
            movement.Npc.position = Vector2.Lerp(movement.Npc.position, movement.NetPosition, 0.2f);
        }
    }
}