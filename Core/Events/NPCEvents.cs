using Terraria;

namespace AshenVoid.Core.Events
{
    public class NPCDamagedEvent : IEvent
    {
        public NPC NPC { get; }
        public NPC.HitInfo Hit { get; }

        public NPCDamagedEvent(NPC npc, NPC.HitInfo hit)
        {
            NPC = npc;
            Hit = hit;
        }
    }

    public class NPCHealthLossEvent : IEvent
    {
        public NPC NPC { get; }
        public float HealthPercentage { get; }
        public float PreviousHealthPercentage { get; }

        public NPCHealthLossEvent(NPC npc, float healthPercentage, float previousHealthPercentage)
        {
            NPC = npc;
            HealthPercentage = healthPercentage;
            PreviousHealthPercentage = previousHealthPercentage;
        }
    }
}