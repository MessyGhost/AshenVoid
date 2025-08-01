using Terraria;
using Microsoft.Xna.Framework;
using AshenVoid.Core.Stats;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.ECS.Intents;
using System.IO;

namespace AshenVoid.Core.ECS
{
    public class MovementComponent : IMovementComponent, INetworkedComponent
    {
        public IMovementIntent CurrentIntent { get; set; }
        public readonly SecondOrderDynamics Dynamics;
        public MovementStats Stats { get; } // Expose stats for other systems to use.

        public Vector2? SyncedPosition { get; private set; }
        public Vector2? SyncedVelocity { get; private set; }

        public MovementComponent(NPC npc, MovementStats stats)
        {
            Stats = stats; // Store the stats.
            Dynamics = new SecondOrderDynamics(stats.Frequency, stats.DampingRatio, stats.ResponseScale);
            Dynamics.Init(npc.Center, npc.velocity);
        }

        public void SetIntent(IMovementIntent intent)
        {
            CurrentIntent = intent;
        }

        public bool IsMoving()
        {
            return CurrentIntent != null && CurrentIntent is not IdleIntent;
        }

        public void SendData(NPC npc, BinaryWriter writer)
        {
            writer.WriteVector2(npc.Center);
            writer.WriteVector2(npc.velocity);
        }

        public void ReceiveData(NPC npc, BinaryReader reader)
        {
            SyncedPosition = reader.ReadVector2();
            SyncedVelocity = reader.ReadVector2();

            Dynamics.Init(SyncedPosition.Value, SyncedVelocity.Value);
        }
    }
}