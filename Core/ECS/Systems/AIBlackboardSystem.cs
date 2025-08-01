using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using AshenVoid.Core.Stats;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AIBlackboardSystem : ISystem
    {
        private bool _isEnraged;
        private const string RAGE_SOURCE = "Rage";

        public AIBlackboardSystem(EventBus eventBus)
        {
            eventBus.Subscribe<NPCDamagedEvent>(OnDamaged);
            eventBus.Subscribe<NPCHealthLossEvent>(OnHealthLoss);
        }

        public void Update()
        {
            // This system is purely event-driven.
        }

        private void OnDamaged(NPCDamagedEvent e)
        {
            var aiState = e.Controller.GetComponent<AIStateComponent>();
            if (aiState == null) return;

            float damageTaken = aiState.Blackboard.Get<float>("DamageTakenSinceLastDash");
            aiState.Blackboard.Set("DamageTakenSinceLastDash", damageTaken + e.Hit.Damage);
        }

        private void OnHealthLoss(NPCHealthLossEvent e)
        {
            var aiState = e.Controller.GetComponent<AIStateComponent>();
            var statSheet = e.Controller.GetComponent<StatSheetComponent>();
            if (aiState == null || statSheet == null) return;

            float lastSummonHealth = aiState.Blackboard.Get<float>("LastSummonHealthPercent");
            if (lastSummonHealth == 0f) lastSummonHealth = 1f;

            if (lastSummonHealth - e.HealthPercentage >= 0.1f)
            {
                aiState.Blackboard.Set("ShouldSummon", true);
                aiState.Blackboard.Set("LastSummonHealthPercent", e.HealthPercentage);
            }

            if (e.HealthPercentage < 0.5f && !_isEnraged)
            {
                _isEnraged = true;
                var damageMod = new StatModifier(0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                var cooldownMod = new StatModifier(-0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                statSheet.Damage.AddModifier(damageMod);
                statSheet.AttackCooldownMultiplier.AddModifier(cooldownMod);
                Main.NewText($"{e.NPC.FullName} has become enraged! Damage and attack speed increased.");
            }
        }
    }
}