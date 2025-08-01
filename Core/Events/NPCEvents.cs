using AshenVoid.Core.ECS;
using Terraria;

namespace AshenVoid.Core.Events
{
    public readonly struct NPCDamagedEvent : IEvent
    {
        public readonly NPC NPC;
        public readonly NPC.HitInfo Hit;
        public readonly IComponentProvider ComponentProvider;

        public NPCDamagedEvent(NPC npc, NPC.HitInfo hit, IComponentProvider componentProvider)
        {
            NPC = npc;
            Hit = hit;
            ComponentProvider = componentProvider;
        }
    }

    public readonly struct NPCHealthLossEvent : IEvent
    {
        public readonly NPC NPC;
        public readonly float HealthPercentage;
        public readonly float PreviousHealthPercentage;
        public readonly IComponentProvider ComponentProvider;

        public NPCHealthLossEvent(NPC npc, float healthPercentage, float previousHealthPercentage, IComponentProvider componentProvider)
        {
            NPC = npc;
            HealthPercentage = healthPercentage;
            PreviousHealthPercentage = previousHealthPercentage;
            ComponentProvider = componentProvider;
        }
    }
}