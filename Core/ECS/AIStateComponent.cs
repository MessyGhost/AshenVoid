using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.Events;
using AshenVoid.Core.Stats;
using Terraria;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Manages the AI's state machine and high-level behavior triggers.
    /// </summary>
    public class AIStateComponent : IComponent
    {
        public readonly NPC NPC;
        public Player Target { get; set; }
        public ComponentController Controller { get; }
        public EventBus EventBus { get; }

        public StateMachine StateMachine { get; }
        private bool _isEnraged;
        private const string RAGE_SOURCE = "Rage";

        // Behavior Triggers
        public bool ShouldDash { get; private set; }
        public bool ShouldSummon { get; set; }
        private float _damageTakenSinceLastDash;
        private float _lastSummonHealthPercent;

        public AIStateComponent(NPC npc, ComponentController controller, EventBus eventBus)
        {
            NPC = npc;
            Controller = controller;
            EventBus = eventBus;
            StateMachine = new StateMachine();
            _isEnraged = false;
            _lastSummonHealthPercent = 1f;
        }


        public void OnDamaged(NPCDamagedEvent e)
        {
            _damageTakenSinceLastDash += e.Hit.Damage;
            // This logic needs to be tied to the config value.
            // For now, we hardcode it. A proper solution would involve the StatSheet.
            if (_damageTakenSinceLastDash >= 300)
            {
                ShouldDash = true;
            }
        }

        public void ResetDashTrigger()
        {
            ShouldDash = false;
            _damageTakenSinceLastDash = 0;
        }

        public void OnHealthLoss(NPCHealthLossEvent e)
        {
            // Summon check
            if (e.PreviousHealthPercentage - e.HealthPercentage >= 0.1f)
            {
                ShouldSummon = true;
                _lastSummonHealthPercent = e.HealthPercentage;
            }

            // Enrage check
            if (e.HealthPercentage < 0.5f && !_isEnraged)
            {
                _isEnraged = true;
                var statSheet = Controller.GetComponent<IStatSheetComponent>();
                if (statSheet != null)
                {
                    // Example: Increase damage by 20% and decrease cooldown by 20%
                    var damageMod = new StatModifier(0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);
                    var cooldownMod = new StatModifier(-0.2f, StatModType.PercentMult, (int)StatModType.PercentMult, RAGE_SOURCE);

                    statSheet.Damage.AddModifier(damageMod);
                    statSheet.AttackCooldownMultiplier.AddModifier(cooldownMod);

                    Main.NewText($"{NPC.FullName} has become enraged! Damage and attack speed increased.");
                }
            }
        }

        public void SetInitialState(IState initialState)
        {
            StateMachine.ChangeState(initialState, Controller, NPC);
        }

        public void ChangeState(IState newState)
        {
            StateMachine.ChangeState(newState, Controller, NPC);
        }

        public void Update()
        {
            // All logic is moved to AIStateSystem.
        }
    }
}