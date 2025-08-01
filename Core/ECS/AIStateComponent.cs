using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AIStateComponent : IComponent
    {
        public StateMachine StateMachine { get; }
        public Blackboard Blackboard { get; }
        
        // Factories are now stored in the Blackboard, making them accessible to states.
        // public StateFactory StateFactory { get; } 

        // For network synchronization
        private readonly Dictionary<Type, int> _stateTypeToId = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> _stateIdToType = new Dictionary<int, Type>();
        private int _nextStateId = 0;

        public AIStateComponent(NPC npc, StateFactory stateFactory, AIBehaviorFactory behaviorFactory)
        {
            StateMachine = new StateMachine();
            Blackboard = new Blackboard();

            // Store dependencies in the blackboard for states and systems to use
            Blackboard.Set(BlackboardKeys.NPC, npc);
            Blackboard.Set(BlackboardKeys.AIState, this);
            Blackboard.Set(BlackboardKeys.StateFactory, stateFactory);
            Blackboard.Set(BlackboardKeys.AIBehaviorFactory, behaviorFactory);
        }

        // Register states to get a unique ID for sync
        public void RegisterState<T>() where T : IState
        {
            var type = typeof(T);
            if (!_stateTypeToId.ContainsKey(type))
            {
                _stateTypeToId[type] = _nextStateId;
                _stateIdToType[_nextStateId] = type;
                _nextStateId++;
            }
        }

        public int GetStateId(Type stateType)
        {
            return _stateTypeToId.TryGetValue(stateType, out var id) ? id : -1;
        }

        public Type GetStateType(int stateId)
        {
            return _stateIdToType.TryGetValue(stateId, out var type) ? type : null;
        }

        public void SetInitialState(Type stateType)
        {
            var stateFactory = Blackboard.Get<StateFactory>(BlackboardKeys.StateFactory);
            var initialState = stateFactory.GetState(stateType);
            StateMachine.ChangeState(initialState, Blackboard);
        }
    }
}