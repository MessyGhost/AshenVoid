using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using System;
using System.Collections.Generic; // Add this
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AIStateComponent : IComponent
    {
        public StateMachine StateMachine { get; }
        public Blackboard Blackboard { get; }
        public StateFactory StateFactory { get; }

        // NEW: For network synchronization
        private readonly Dictionary<Type, int> _stateTypeToId = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> _stateIdToType = new Dictionary<int, Type>();
        private int _nextStateId = 0;

        public AIStateComponent(NPC npc, StateFactory stateFactory)
        {
            StateMachine = new StateMachine();
            Blackboard = new Blackboard();
            StateFactory = stateFactory;

            Blackboard.Set(BlackboardKeys.NPC, npc);
            Blackboard.Set(BlackboardKeys.AIState, this);
        }

        // NEW: Register states to get a unique ID for sync
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
            var initialState = StateFactory.GetState(stateType);
            StateMachine.ChangeState(initialState, Blackboard);
        }
    }
}