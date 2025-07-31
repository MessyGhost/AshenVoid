using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS.Systems
{
    public class MovementSystem : ISystem
    {
        public void Update(GameTime gameTime, NPC npc)
        {
            var controller = (npc.ModNPC as IComponentProvider)?.ComponentController;
            if (controller == null) return;

            var movementComponent = controller.GetComponent<MovementComponent>();
            if (movementComponent == null) return;

            Vector2 targetVelocity = npc.velocity;
            Vector2 destination = npc.Center;

            switch (movementComponent.CurrentIntent)
            {
                case ChaseIntent chase:
                    destination = chase.TargetPosition;
                    if (npc.Center.Distance(chase.TargetPosition) < chase.StopDistance)
                    {
                        destination = npc.Center;
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
                    movementComponent.CurrentIntent = null;
                    return;

                case IdleIntent _:
                case null:
                    npc.velocity *= 0.95f;
                    movementComponent.CurrentIntent = null;
                    return;
            }

            Vector2 idealPosition = movementComponent.Dynamics.Update((float)gameTime.ElapsedGameTime.TotalSeconds, npc.Center, destination);
            targetVelocity = idealPosition - npc.Center;

            npc.velocity = Collision.TileCollision(npc.position, targetVelocity, npc.width, npc.height, !npc.noTileCollide, !npc.noTileCollide);
        }
    }
}