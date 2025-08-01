using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class StatSystem : IComponentSystem
    {
        public IEnumerable<Type> RequiredComponents => new[] { typeof(StatSheetComponent) };
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public void Update(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var stats = world.GetComponent<StatSheetComponent>(entityId);
            // Stat update logic, if any
        }
    }
}