using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using AshenVoid.Core.Events;
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

        public AIComponent(NPC npc, ComponentController controller, EventBus eventBus)
        {
            NPC = npc;
            Controller = controller;
            _eventBus = eventBus;
            _stateMachine = new StateMachine(this);
        }

        public void Initialize()
        {
            _eventBus.Subscribe<NPCDamagedEvent>(OnDamaged);
            _eventBus.Subscribe<NPCHealthLossEvent>(OnHealthLoss);
        }

        private void OnDamaged(NPCDamagedEvent e)
        {
            // Placeholder for future logic, e.g., triggering a counter-attack.
            // For now, we can log it for debugging.
            Main.NewText($"AIComponent received NPCDamagedEvent: {e.Hit.Damage} damage.");
        }

        private void OnHealthLoss(NPCHealthLossEvent e)
        {
            // Placeholder for future logic, e.g., checking for phase transitions.
            Main.NewText($"AIComponent received NPCHealthLossEvent: Health is now {e.HealthPercentage:P2}.");
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