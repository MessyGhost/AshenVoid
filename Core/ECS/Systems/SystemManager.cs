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
                if (system is IComponentSystem componentSystem)
                {
                    var side = componentSystem.ExecutionSide;
                    if ((side == SystemExecutionSide.Server && Main.netMode == NetmodeID.MultiplayerClient) ||
                        (side == SystemExecutionSide.Client && Main.netMode == NetmodeID.Server))
                    {
                        continue;
                    }

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