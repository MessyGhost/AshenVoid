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
        private readonly StateFactory _stateFactory;
        public IState CurrentState { get; private set; }

        public AIStateComponent(Type initialState, StateFactory stateFactory)
        {
            _stateFactory = stateFactory;
            CurrentState = _stateFactory.CreateState(initialState);
        }

        public void Update(GameTime gameTime, EcsWorld world, int entityId, EventBus eventBus)
        {
            CurrentState?.Update(entityId, world);

            var nextStateType = CurrentState?.CheckTransitions(entityId, world);
            if (nextStateType != null)
            {
                var nextState = _stateFactory.CreateState(nextStateType);
                if (nextState != null && nextState.GetType() != CurrentState.GetType())
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