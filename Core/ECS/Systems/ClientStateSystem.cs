using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class ClientStateSystem : EntityQuerySystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(AIStateComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public ClientStateSystem()
        {
            EcsSystem.Instance.EventBus.Subscribe<StateChangedNetworkEvent>(HandleStateChange);
        }

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            // This system only reacts to network events, so no per-frame update logic is needed.
        }

        private void HandleStateChange(StateChangedNetworkEvent e)
        {
            var world = EcsSystem.Instance.World;
            var aiState = world.GetComponent<AIStateComponent>(e.EntityId);
            if (aiState == null) return;

            var stateFactory = EcsSystem.Instance.StateFactory;
            Type stateType = stateFactory.GetTypeById(e.StateId);
            if (stateType != null)
            {
                // Create a new instance of the state and force it.
                var nextState = stateFactory.CreateState(stateType);
                if (nextState != null && nextState.GetType() != aiState.CurrentState.GetType())
                {
                    // We need to call Enter/Exit manually on the client for synchronization
                    aiState.CurrentState?.Exit(e.EntityId, world);
                    aiState.ForceState(nextState);
                    // Client does not have or need the factory, so pass null.
                    aiState.CurrentState.Enter(e.EntityId, world, null);
                }
            }
        }
    }
}