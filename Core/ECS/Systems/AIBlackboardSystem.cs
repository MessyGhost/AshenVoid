using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using AshenVoid.Core.Stats;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AIBlackboardSystem : ISystem
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

        public void Update()
        {
            // This system is purely event-driven.
        }

        private void OnDamaged(NPCDamagedEvent e)
        {
            if (!e.ComponentProvider.TryGetComponent(out AIStateComponent aiState)) return;

            float damageTaken = aiState.Blackboard.Get<float>("DamageTakenSinceLastDash");
            aiState.Blackboard.Set("DamageTakenSinceLastDash", damageTaken + e.Hit.Damage);
        }

        private void OnHealthLoss(NPCHealthLossEvent e)
        {
            if (!e.ComponentProvider.TryGetComponent(out AIStateComponent aiState) || !e.ComponentProvider.TryGetComponent(out StatSheetComponent statSheet))
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
                var damageMod = new StatModifier(0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                var cooldownMod = new StatModifier(-0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                statSheet.Damage.AddModifier(damageMod);
                statSheet.AttackCooldownMultiplier.AddModifier(cooldownMod);
                Main.NewText($"{e.NPC.FullName} has become enraged! Damage and attack speed increased.");
            }
        }
    }
}