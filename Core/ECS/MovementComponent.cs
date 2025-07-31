using Terraria;
using Microsoft.Xna.Framework;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.ECS.Intents;

namespace AshenVoid.Core.ECS
{
    public class MovementComponent : IMovementComponent, IComponent
    {
        public IMovementIntent CurrentIntent { get; set; }
        public readonly SecondOrderDynamics Dynamics;

        public MovementComponent(NPC npc, MovementStats stats)
        {
            Dynamics = new SecondOrderDynamics(stats.Frequency, stats.DampingRatio, stats.ResponseScale);
        }

        public void SetIntent(IMovementIntent intent)
        {
            CurrentIntent = intent;
        }

        public bool IsMoving()
        {
            return CurrentIntent != null && CurrentIntent is not IdleIntent;
        }
    }
}