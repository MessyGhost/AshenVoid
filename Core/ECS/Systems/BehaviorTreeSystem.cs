using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.Systems
{
    public class BehaviorTreeSystem : EntityQuerySystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(AIStateComponent), typeof(AIBlackboardComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var blackboard = world.GetComponent<AIBlackboardComponent>(entityId);
            if (blackboard.FindTargetCooldown > 0)
            {
                blackboard.FindTargetCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            var aiState = world.GetComponent<AIStateComponent>(entityId);
            aiState?.CurrentState?.BehaviorTree?.Tick(entityId, world);
        }
    }
}