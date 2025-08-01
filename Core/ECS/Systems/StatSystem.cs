using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class StatSystem : EntityQuerySystem
    {
        public override IEnumerable<Type> RequiredComponents => new[] { typeof(StatSheetComponent) };
        public override SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public override void UpdateEntity(GameTime gameTime, int entityId, EcsWorld world, EventBus eventBus)
        {
            var stats = world.GetComponent<StatSheetComponent>(entityId);
            // Stat update logic, if any
        }
    }
}