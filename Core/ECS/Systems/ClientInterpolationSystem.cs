using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class ClientInterpolationSystem : IComponentSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public HashSet<Type> RequiredComponents { get; } = new HashSet<Type>
        {
            typeof(MovementComponent)
        };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            var movement = controller.GetComponent<MovementComponent>();
            if (movement == null) return;

            // On the client, we don't calculate movement. We interpolate to the state
            // received from the server to ensure smooth visuals.
            if (movement.SyncedPosition.HasValue)
            {
                // Use the dynamics to smoothly move towards the server-authoritative position.
                // This prevents jitter and makes movement look natural despite network latency.
                npc.Center = movement.Dynamics.Update((float)gameTime.ElapsedGameTime.TotalSeconds, movement.SyncedPosition.Value, movement.SyncedVelocity);
            }
        }
    }
}