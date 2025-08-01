using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using AshenVoid.Core.Events;

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<IComponentSystem> _systems = new List<IComponentSystem>();

        public void RegisterSystem(ISystem system)
        {
            if (system is IComponentSystem componentSystem)
            {
                _systems.Add(componentSystem);
            }
        }

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            foreach (var system in _systems)
            {
                // Check if the controller has all the components required by the system.
                bool canRun = true;
                foreach (var requiredComponentType in system.RequiredComponents)
                {
                    if (!controller.HasComponent(requiredComponentType))
                    {
                        canRun = false;
                        break;
                    }
                }

                if (canRun)
                {
                    system.Update(gameTime, npc, controller, eventBus);
                }
            }
        }
    }
}