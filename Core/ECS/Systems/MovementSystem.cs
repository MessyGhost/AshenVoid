using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.Intents;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class MovementSystem : IMovementSystem
    {
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
                npc.velocity *= 0.95f;
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
                    npc.velocity *= 0.95f;
                    blackboard.Remove(BlackboardKeys.MovementIntent);
                    return;
            }

            Vector2 idealPosition = movementComponent.Dynamics.Update((float)gameTime.ElapsedGameTime.TotalSeconds, npc.Center, destination);
            Vector2 targetVelocity = idealPosition - npc.Center;

            npc.velocity = Collision.TileCollision(npc.position, targetVelocity, npc.width, npc.height, !npc.noTileCollide, !npc.noTileCollide);
        }
    }
}