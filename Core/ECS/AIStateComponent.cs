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
        public StateFactory StateFactory { get; } // Expose the factory

        public AIStateComponent(NPC npc, StateFactory stateFactory)
        {
            StateMachine = new StateMachine();
            Blackboard = new Blackboard();
            StateFactory = stateFactory; // Store the factory

            // Populate the blackboard with long-lived objects
            Blackboard.Set(BlackboardKeys.NPC, npc);
            Blackboard.Set(BlackboardKeys.AIState, this);
        }

        public void SetInitialState(Type stateType)
        {
            var initialState = StateFactory.GetState(stateType);
            StateMachine.ChangeState(initialState, Blackboard);
        }
    }
}