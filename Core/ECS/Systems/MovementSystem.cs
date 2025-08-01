using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.Intents;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AIStateSystem))]
    public class MovementSystem : IComponentSystem
    {
        private const float Friction = 0.95f; // Damping factor for idle movement.

        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public HashSet<Type> RequiredComponents { get; } = new HashSet<Type>
        {
            typeof(MovementComponent),
            typeof(AIStateComponent)
        };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            var movementComponent = controller.GetComponent<MovementComponent>();
            var aiState = controller.GetComponent<AIStateComponent>();
            var blackboard = aiState.Blackboard;

            if (!blackboard.TryGet(BlackboardKeys.MovementIntent, out IMovementIntent movementIntent))
            {
                npc.velocity *= Friction;
                return;
            }

            Vector2 destination = npc.Center;

            switch (movementIntent)
            {
                case ChaseIntent chase:
                    destination = chase.TargetPosition;
                    if (npc.Center.Distance(chase.TargetPosition) < chase.StopDistance)
                    {
                        destination = npc.Center;
                        blackboard.Remove(BlackboardKeys.MovementIntent);
                    }
                    break;

                case OrbitIntent orbit:
                    float angle = (float)(gameTime.TotalGameTime.TotalSeconds * 1.5f) * orbit.Direction;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbit.Radius;
                    destination = orbit.Center + offset;
                    break;

                case FleeIntent flee:
                    Vector2 fleeDirection = npc.Center - flee.FleeFromPosition;
                    if (fleeDirection == Vector2.Zero) fleeDirection = Vector2.UnitY;
                    fleeDirection.Normalize();
                    destination = npc.Center + fleeDirection * 200f;
                    break;

                case TeleportIntent teleport:
                    npc.Center = teleport.TargetPosition;
                    npc.velocity = Vector2.Zero;
                    blackboard.Remove(BlackboardKeys.MovementIntent);
                    return;

                case IdleIntent _:
                    npc.velocity *= Friction;
                    blackboard.Remove(BlackboardKeys.MovementIntent);
                    return;
            }
            
            Vector2 targetVelocity = movementComponent.Dynamics.Update((float)gameTime.ElapsedGameTime.TotalSeconds, destination) - npc.Center;
            
            if (targetVelocity.Length() > movementComponent.Stats.MaxSpeed)
            {
                targetVelocity.Normalize();
                targetVelocity *= movementComponent.Stats.MaxSpeed;
            }

            npc.velocity = targetVelocity;
        }
    }
}