using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AIStateComponent : IComponent
    {
        public StateMachine StateMachine { get; }
        public Blackboard Blackboard { get; }
        private readonly StateFactory _stateFactory;

        public AIStateComponent(NPC npc, StateFactory stateFactory)
        {
            StateMachine = new StateMachine();
            Blackboard = new Blackboard();
            _stateFactory = stateFactory;

            // Populate the blackboard with long-lived objects
            Blackboard.Set(BlackboardKeys.NPC, npc);
            Blackboard.Set(BlackboardKeys.AIState, this);
        }

        public void SetInitialState(Type stateType)
        {
            var initialState = _stateFactory.GetState(stateType);
            StateMachine.ChangeState(initialState, Blackboard);
        }

        public void ChangeState<T>() where T : class, IState, new()
        {
            var newState = _stateFactory.GetState<T>();
            StateMachine.ChangeState(newState, Blackboard);
        }
    }
}