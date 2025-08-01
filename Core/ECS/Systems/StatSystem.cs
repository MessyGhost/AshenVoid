using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class StatSystem : IComponentSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;

        public HashSet<Type> RequiredComponents { get; } = new HashSet<Type>
        {
            typeof(StatSheetComponent)
        };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            var statSheet = controller.GetComponent<StatSheetComponent>();
            // Logic for updating stats over time would go here.
            // For example, applying buffs/debuffs that modify stats.
        }
    }
}