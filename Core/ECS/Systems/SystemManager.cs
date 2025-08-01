using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<ISystem> _systems = new List<ISystem>();

        public void RegisterSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void Update(GameTime gameTime, EcsWorld world, EventBus eventBus)
        {
            foreach (var system in _systems)
            {
                // Check execution side first
                if (system is IComponentSystem componentSystemBase)
                {
                    var side = componentSystemBase.ExecutionSide;
                    if ((side == SystemExecutionSide.Server && Main.netMode == NetmodeID.MultiplayerClient) ||
                        (side == SystemExecutionSide.Client && Main.netMode == NetmodeID.Server))
                    {
                        continue;
                    }
                }

                // New logic for cached systems
                if (system is CachedComponentSystem cachedSystem)
                {
                    cachedSystem.UpdateAll(gameTime, world, eventBus);
                }
                // Old logic for non-cached systems
                else if (system is IComponentSystem componentSystem)
                {
                    var entities = world.GetEntities(componentSystem.RequiredComponents);
                    foreach (var entityId in entities)
                    {
                        componentSystem.Update(gameTime, entityId, world, eventBus);
                    }
                }
            }
        }
    }
}