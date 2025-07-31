using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.Events;
using AshenVoid.Core.Stats;
using Terraria;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// 唯一职责是持有并更新状态机 (StateMachine)。
    /// 不再管理或直接引用其他组件。
    /// </summary>
    public class AIComponent : IComponent, IInitializable
    {
        public readonly NPC NPC;
        public Player Target { get; private set; }
        public ComponentController Controller { get; }

        private readonly StateMachine _stateMachine;
        private readonly EventBus _eventBus;
        private bool _isEnraged;
        private const string RAGE_SOURCE = "Rage";

        public AIComponent(NPC npc, ComponentController controller, EventBus eventBus)
        {
            NPC = npc;
            Controller = controller;
            _eventBus = eventBus;
            _stateMachine = new StateMachine(this);
            _isEnraged = false;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<NPCDamagedEvent>(OnDamaged);
            _eventBus.Subscribe<NPCHealthLossEvent>(OnHealthLoss);
        }

        private void OnDamaged(NPCDamagedEvent e)
        {
            // Placeholder for future logic, e.g., triggering a counter-attack.
        }

        private void OnHealthLoss(NPCHealthLossEvent e)
        {
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
            _stateMachine.ChangeState(initialState);
        }

        public void ChangeState(IState newState)
        {
            _stateMachine.ChangeState(newState);
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
            _stateMachine.Update();
        }
    }
}