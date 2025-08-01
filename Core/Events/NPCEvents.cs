using Terraria;

namespace AshenVoid.Core.Events
{
    /// <summary>
    /// Published when an NPC is damaged.
    /// Systems can listen to this to react to damage, e.g., by changing stats or behavior.
    /// The ComponentController can be accessed via the NPC instance if it's an EcsBoss.
    /// </summary>
    public readonly struct NPCDamagedEvent : IEvent
    {
        public readonly NPC NPC;
        public readonly NPC.HitInfo Hit;

        public NPCDamagedEvent(NPC npc, NPC.HitInfo hit)
        {
            NPC = npc;
            Hit = hit;
        }
    }

    /// <summary>
    /// Published by the HealthSystem when an NPC's health changes.
    /// </summary>
    public readonly struct NPCHealthLossEvent : IEvent
    {
        public readonly NPC NPC;
        public readonly float HealthPercentage;
        public readonly float PreviousHealthPercentage;

        public NPCHealthLossEvent(NPC npc, float healthPercentage, float previousHealthPercentage)
        {
            NPC = npc;
            HealthPercentage = healthPercentage;
            PreviousHealthPercentage = previousHealthPercentage;
        }
    }
}