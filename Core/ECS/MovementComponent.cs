using Terraria;
using Microsoft.Xna.Framework;
using System;
using AshenVoid.Core;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.Interfaces;

namespace AshenVoid.Core.ECS
{
    public class MovementComponent : IMovementComponent
    {
        private readonly NPC _npc;
        private IMovementIntent _currentIntent;
        private readonly SecondOrderDynamics _dynamics;

        public MovementComponent(NPC npc, MovementStats stats)
        {
            _npc = npc;
            _dynamics = new SecondOrderDynamics(stats.Frequency, stats.DampingRatio, stats.ResponseScale);
        }

        /// <summary>
        /// Sets the movement goal for the current frame. Called by the Behavior Tree.
        /// </summary>
        public void SetIntent(IMovementIntent intent)
        {
            _currentIntent = intent;
        }

        public bool IsMoving()
        {
            // We consider any intent that isn't idle as a "moving" state.
            return _currentIntent != null && _currentIntent is not IdleIntent;
        }

        /// <summary>
        /// Called by the ComponentController every frame to execute movement logic.
        /// </summary>
        public void Update()
        {
            Vector2 targetVelocity = _npc.velocity;
            Vector2 destination = _npc.Center; // Default to current position

            switch (_currentIntent)
            {
                case ChaseIntent chase:
                    destination = chase.TargetPosition;
                    // Optional: If close enough, just hover instead of chasing
                    if (_npc.Center.Distance(chase.TargetPosition) < chase.StopDistance)
                    {
                        destination = _npc.Center;
                    }
                    break;

                case OrbitIntent orbit:
                    float angle = (float)(DateTime.Now.TimeOfDay.TotalSeconds * 1.5f) * orbit.Direction;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbit.Radius;
                    destination = orbit.Center + offset;
                    break;

                case FleeIntent flee:
                    Vector2 fleeDirection = _npc.Center - flee.FleeFromPosition;
                    if (fleeDirection == Vector2.Zero) fleeDirection = Vector2.UnitY; // Flee upwards if on top
                    fleeDirection.Normalize();
                    destination = _npc.Center + fleeDirection * 200f; // Flee to a point 200px away
                    break;

                case IdleIntent _:
                case null:
                    // No intent, so we just apply damping to the current velocity.
                    _npc.velocity *= 0.95f;
                    _currentIntent = null;
                    return;
            }

            // Use the PID controller to calculate the ideal position for this frame
            Vector2 idealPosition = _dynamics.Update((float)Main.time / 60f, _npc.Center, destination);
            targetVelocity = idealPosition - _npc.Center;

            // Apply collision detection to the calculated velocity
            _npc.velocity = Collision.TileCollision(_npc.position, targetVelocity, _npc.width, _npc.height, !_npc.noTileCollide, !_npc.noTileCollide);
        }
    }
}