using AshenVoid.Core.Builders;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using AshenVoid.Core.Stats;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    // This system is event-driven and manages blackboard values based on game events.
    public class AIBlackboardSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        private bool _isEnraged;
        private const string RAGE_SOURCE = "Rage";
        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<NPCDamagedEvent>(OnDamaged);
            _eventBus.Subscribe<NPCHealthLossEvent>(OnHealthLoss);
        }

        public void Shutdown()
        {
            _eventBus.Unsubscribe<NPCDamagedEvent>(OnDamaged);
            _eventBus.Unsubscribe<NPCHealthLossEvent>(OnHealthLoss);
        }

        private void OnDamaged(NPCDamagedEvent e)
        {
            if (e.NPC.ModNPC is not EcsBoss boss) return;
            if (!boss.Controller.TryGetComponent(out AIStateComponent aiState)) return;

            float damageTaken = aiState.Blackboard.Get<float>("DamageTakenSinceLastDash");
            aiState.Blackboard.Set("DamageTakenSinceLastDash", damageTaken + e.Hit.Damage);
        }

        private void OnHealthLoss(NPCHealthLossEvent e)
        {
            if (e.NPC.ModNPC is not EcsBoss boss) return;
            if (!boss.Controller.TryGetComponent(out AIStateComponent aiState) || !boss.Controller.TryGetComponent(out StatSheetComponent statSheet))
            {
                return;
            }

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
                // Correctly creating the StatModifier with 4 arguments.
                var damageMod = new StatModifier(0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                var cooldownMod = new StatModifier(-0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                statSheet.Damage.AddModifier(damageMod);
                statSheet.AttackCooldownMultiplier.AddModifier(cooldownMod);
                Main.NewText($"{e.NPC.FullName} has become enraged! Damage and attack speed increased.");
            }
        }
    }
}