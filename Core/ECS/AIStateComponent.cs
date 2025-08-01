using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AIStateComponent : IComponent
    {
        private readonly NPC _npc;
        private readonly Dictionary<byte, IState> _states = new();
        private readonly StateFactory _stateFactory;
        public IState CurrentState { get; private set; }

        public AIStateComponent(NPC npc, StateFactory stateFactory)
        {
            _npc = npc;
            _stateFactory = stateFactory;
        }

        public void RegisterState(IState state)
        {
            var stateId = _stateFactory.GetIdByType(state.GetType());
            _states[stateId] = state;
        }

        public IState GetState(Type stateType)
        {
            var stateId = _stateFactory.GetIdByType(stateType);
            _states.TryGetValue(stateId, out var state);
            return state;
        }

        public void SetInitialState(Type stateType)
        {
            var stateId = _stateFactory.GetIdByType(stateType);
            if (_states.ContainsKey(stateId))
            {
                CurrentState = _states[stateId];
            }
        }

        public void Update(GameTime gameTime, EcsWorld world, int entityId, EventBus eventBus)
        {
            CurrentState?.Update(entityId, world);

            var nextStateType = CurrentState?.CheckTransitions(entityId, world);
            if (nextStateType != null)
            {
                var nextStateId = _stateFactory.GetIdByType(nextStateType);
                if (_states.TryGetValue(nextStateId, out var nextState) && nextState != CurrentState)
                {
                    ChangeState(entityId, world, eventBus, nextState);
                }
            }
        }

        private void ChangeState(int entityId, EcsWorld world, EventBus eventBus, IState nextState)
        {
            CurrentState?.Exit(entityId, world);
            CurrentState = nextState;
            CurrentState.Enter(entityId, world);

            if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
            {
                eventBus.Publish(new StateChangedNetworkEvent
                {
                    EntityId = entityId,
                    StateId = _stateFactory.GetIdByType(nextState.GetType())
                });
            }
        }

        /// <summary>
        /// Forcibly sets the current state. Used by the client to sync with the server.
        /// </summary>
        public void ForceState(IState state)
        {
            CurrentState = state;
        }
    }
}