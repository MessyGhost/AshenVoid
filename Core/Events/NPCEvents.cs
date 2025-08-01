using AshenVoid.Core.ECS;
using Terraria;

namespace AshenVoid.Core.Events
{
    public readonly struct NPCDamagedEvent : IEvent
    {
        public readonly ComponentController Controller;
        public readonly NPC NPC;
        public readonly NPC.HitInfo Hit;

        public NPCDamagedEvent(ComponentController controller, NPC npc, NPC.HitInfo hit)
        {
            Controller = controller;
            NPC = npc;
            Hit = hit;
        }
    }

    public readonly struct NPCHealthLossEvent : IEvent
    {
        public readonly ComponentController Controller;
        public readonly NPC NPC;
        public readonly float HealthPercentage;
        public readonly float PreviousHealthPercentage;

        public NPCHealthLossEvent(ComponentController controller, NPC npc, float healthPercentage, float previousHealthPercentage)
        {
            Controller = controller;
            NPC = npc;
            HealthPercentage = healthPercentage;
            PreviousHealthPercentage = previousHealthPercentage;
        }
    }
}