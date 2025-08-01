using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AIStateComponent : IComponent
    {
        public readonly NPC NPC;
        public ComponentController Controller { get; }
        public Blackboard Blackboard { get; }
        public StateMachine StateMachine { get; }
        private readonly StateFactory _stateFactory;

        public AIStateComponent(NPC npc, ComponentController controller, StateFactory stateFactory)
        {
            NPC = npc;
            Controller = controller;
            StateMachine = new StateMachine();
            Blackboard = new Blackboard();
            _stateFactory = stateFactory;
        }

        public void SetInitialState(Type stateType)
        {
            Blackboard.Set(BlackboardKeys.NPC, NPC);
            Blackboard.Set(BlackboardKeys.Controller, Controller);
            Blackboard.Set(BlackboardKeys.AIState, this);

            var state = _stateFactory.GetState(stateType);
            StateMachine.ChangeState(state, Blackboard);
        }

        public void ChangeState<T>() where T : class, IState, new()
        {
            var newState = _stateFactory.GetState<T>();
            StateMachine.ChangeState(newState, Blackboard);
        }
    }
}