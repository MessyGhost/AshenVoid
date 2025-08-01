using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
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

        public void BuildCache(ComponentController controller) { }

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            foreach (var system in _systems)
            {
                if (system is IComponentSystem componentSystem)
                {
                    if (!componentSystem.RequiredComponents.All(controller.HasComponent))
                    {
                        continue;
                    }

                    var side = componentSystem.ExecutionSide;
                    if ((side == SystemExecutionSide.Server && Main.netMode == NetmodeID.MultiplayerClient) ||
                        (side == SystemExecutionSide.Client && Main.netMode == NetmodeID.Server))
                    {
                        continue;
                    }

                    componentSystem.Update(gameTime, npc, controller, eventBus);
                }
            }
        }
    }
}