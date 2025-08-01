using AshenVoid.Core.Stats;
using AshenVoid.Core.Utility;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class MovementComponent : IComponent
    {
        public NPC Npc { get; }
        public SecondOrderDynamics Dynamics { get; }
        public Vector2 TargetPosition { get; set; }
        public Vector2 NetPosition { get; set; }

        public MovementComponent(NPC npc, MovementStats stats)
        {
            Npc = npc;
            Dynamics = new SecondOrderDynamics(stats.Frequency, stats.DampingRatio, stats.ResponseScale, npc.Center);
            TargetPosition = npc.Center;
        }
    }
}