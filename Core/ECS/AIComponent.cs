using AshenVoid.Core.DI;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AIComponent : IComponent
    {
        public readonly NPC NPC;
        public Player Target { get; private set; }

        private readonly ServiceContainer _serviceContainer;
        private readonly StateMachine _stateMachine;

        // 移除直接引用，改为通过DI获取
        private IMovementComponent _movementComponent;
        private IAttackComponent _attackComponent;
        private IAnimationComponent _animationComponent;
        private IVFXComponent _vfxComponent;

        public AIComponent(NPC npc, ServiceContainer serviceContainer)
        {
            NPC = npc;
            _serviceContainer = serviceContainer;
            _stateMachine = new StateMachine(this);
        }

        public void Initialize()
        {
            // 通过DI获取依赖
            _movementComponent = _serviceContainer.GetService<IMovementComponent>();
            _attackComponent = _serviceContainer.GetService<IAttackComponent>();
            _animationComponent = _serviceContainer.GetService<IAnimationComponent>();
            _vfxComponent = _serviceContainer.GetService<IVFXComponent>();
        }

        public void SetInitialState(IState initialState)
        {
            _stateMachine.ChangeState(initialState);
        }

        // 提供访问器方法而非直接暴露字段
        public IMovementComponent GetMovementComponent() => _movementComponent;
        public IAttackComponent GetAttackComponent() => _attackComponent;
        public IAnimationComponent GetAnimationComponent() => _animationComponent;
        public IVFXComponent GetVFXComponent() => _vfxComponent;

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

            // Update the state machine, which in turn updates the behavior tree
            _stateMachine.Update();
        }
    }
}