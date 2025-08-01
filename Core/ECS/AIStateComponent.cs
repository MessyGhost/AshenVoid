using AshenVoid.Core.Builders;
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

        public void CheckTransitionsAndChangeState(EcsWorld world, int entityId, EventBus eventBus)
        {
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

            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            var boss = statSheet?.Npc.ModNPC as EcsBoss;
            CurrentState.Enter(entityId, world, boss?.AiFactory);

            if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
            {
                var networkEvent = new StateChangedNetworkEvent
                {
                    EntityId = entityId,
                    StateId = _stateFactory.GetIdByType(nextState.GetType())
                };
                // Publish locally for server-side systems to react
                eventBus.Publish(networkEvent);
                // Send over the network explicitly
                EcsSystem.Instance.NetworkManager.Send(networkEvent);
            }
        }

        public void ForceState(IState state)
        {
            CurrentState = state;
        }
    }
}