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
        private readonly Dictionary<Type, IState> _states = new();
        public IState CurrentState { get; private set; }

        public AIStateComponent(NPC npc)
        {
            _npc = npc;
        }

        public void RegisterState(IState state)
        {
            _states[state.GetType()] = state;
        }

        public IState GetState(Type stateType)
        {
            _states.TryGetValue(stateType, out var state);
            return state;
        }

        public void SetInitialState(Type stateType)
        {
            if (_states.ContainsKey(stateType))
            {
                CurrentState = _states[stateType];
            }
        }

        public void Update(GameTime gameTime, EcsWorld world, int entityId, EventBus eventBus)
        {
            CurrentState?.Update(entityId, world);

            var nextStateType = CurrentState?.CheckTransitions(entityId, world);
            if (nextStateType != null && _states.TryGetValue(nextStateType, out var nextState))
            {
                if (nextState != CurrentState)
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

            // Publish the state change to clients
            if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
            {
                eventBus.Publish(new StateChangedNetworkEvent
                {
                    EntityId = entityId,
                    StateTypeName = nextState.GetType().AssemblyQualifiedName
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