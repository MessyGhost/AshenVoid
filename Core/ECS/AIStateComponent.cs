using AshenVoid.Core.ECS.AI;
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

        // For network synchronization
        private readonly Dictionary<Type, int> _stateTypeToId = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> _stateIdToType = new Dictionary<int, Type>();
        private int _nextStateId = 0;

        public AIStateComponent(NPC npc)
        {
            StateMachine = new StateMachine();
            Blackboard = new Blackboard();

            // Store core references in the blackboard
            Blackboard.Set(BlackboardKeys.NPC, npc);
            Blackboard.Set(BlackboardKeys.AIState, this);
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
            // Services are now retrieved from the blackboard, which is populated by the EcsBoss
            var services = Blackboard.Get<ServiceLocator>(BlackboardKeys.ServiceLocator);
            var stateFactory = services.Get<StateFactory>();
            var initialState = stateFactory.GetState(stateType);
            StateMachine.ChangeState(initialState, Blackboard);
        }
    }
}