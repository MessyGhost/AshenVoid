using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class AttackSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(AttackComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var attack = world.GetComponent<AttackComponent>(entityId);
            // Attack logic will be implemented later
        }
    }
}