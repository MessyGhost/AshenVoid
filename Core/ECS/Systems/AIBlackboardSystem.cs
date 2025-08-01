using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AIBlackboardSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(AIStateComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            // This system's purpose was likely to manage the blackboard.
            // With the new design, the blackboard is managed within the AIStateComponent and States.
            // This system might be redundant now, but we'll keep it for now and implement the interface correctly.
        }
    }
}