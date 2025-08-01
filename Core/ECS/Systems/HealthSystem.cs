using AshenVoid.Core.Events;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class HealthSystem : IHealthSystem
    {
        public void Update(NPC npc, HealthComponent health, EventBus eventBus)
        {
            if (npc.life != health.LastHealth)
            {
                if (npc.ModNPC is IComponentProvider provider)
                {
                    float lastHealthPercent = (float)health.LastHealth / npc.lifeMax;
                    float currentHealthPercent = (float)npc.life / npc.lifeMax;
                    eventBus.Publish(new NPCHealthLossEvent(npc, currentHealthPercent, lastHealthPercent, provider));
                    health.LastHealth = npc.life;
                }
            }
        }
    }
}