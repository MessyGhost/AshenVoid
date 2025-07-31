using AshenVoid.Core.ECS.FSM;
using Terraria;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// 唯一职责是持有并更新状态机 (StateMachine)。
    /// 不再管理或直接引用其他组件。
    /// </summary>
    public class AIComponent : IComponent
    {
        public readonly NPC NPC;
        public Player Target { get; private set; }
        public ComponentController Controller { get; }

        private readonly StateMachine _stateMachine;

        public AIComponent(NPC npc, ComponentController controller)
        {
            NPC = npc;
            Controller = controller;
            _stateMachine = new StateMachine(this);
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