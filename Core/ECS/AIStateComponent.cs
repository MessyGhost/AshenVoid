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
    public class AIStateComponent : IComponent, IInitializable
    {
        public readonly NPC NPC;
        public Player Target { get; private set; }
        public ComponentController Controller { get; }
        public EventBus EventBus { get; }

        private readonly StateMachine _stateMachine;
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
            _stateMachine = new StateMachine();
            _isEnraged = false;
            _lastSummonHealthPercent = 1f;
        }

        public void Initialize()
        {
            EventBus.Subscribe<NPCDamagedEvent>(OnDamaged);
            EventBus.Subscribe<NPCHealthLossEvent>(OnHealthLoss);
        }

        private void OnDamaged(NPCDamagedEvent e)
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

        private void OnHealthLoss(NPCHealthLossEvent e)
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
            _stateMachine.ChangeState(initialState, Controller, NPC);
        }

        public void ChangeState(IState newState)
        {
            _stateMachine.ChangeState(newState, Controller, NPC);
        }

        public void Update()
        {
            // Update target
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(true);
            }
            Target = Main.player[NPC.target];

            // Update the state machine
            _stateMachine.Update(Controller, NPC, Target);
        }
    }
}