using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class ClientStateSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(AIStateComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Client;

        public ClientStateSystem()
        {
            EcsSystem.Instance.EventBus.Subscribe<StateChangedNetworkEvent>(HandleStateChange);
        }

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            // This system only reacts to network events, so no per-frame update logic is needed.
        }

        private void HandleStateChange(StateChangedNetworkEvent e)
        {
            var world = EcsSystem.Instance.World;
            var aiState = world.GetComponent<AIStateComponent>(e.EntityId);
            if (aiState == null) return;

            Type stateType = Type.GetType(e.StateTypeName);
            if (stateType != null)
            {
                var nextState = aiState.GetState(stateType);
                if (nextState != null && nextState != aiState.CurrentState)
                {
                    aiState.ForceState(nextState);
                }
            }
        }
    }
}