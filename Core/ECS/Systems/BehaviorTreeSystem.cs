using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.Systems
{
    public class BehaviorTreeSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(AIStateComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var aiState = world.GetComponent<AIStateComponent>(entityId);
            aiState?.CurrentState?.BehaviorTree?.Tick(entityId, world);
        }
    }
}