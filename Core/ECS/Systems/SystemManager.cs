using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using AshenVoid.Core.Events;
using Terraria.ID; // Add this

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<IComponentSystem> _systems = new List<IComponentSystem>();

        // NEW: Cache for system run eligibility
        private readonly Dictionary<IComponentSystem, bool> _systemEligibilityCache = new Dictionary<IComponentSystem, bool>();
        private bool _isCacheDirty = true;

        public void RegisterSystem(ISystem system)
        {
            if (system is IComponentSystem componentSystem)
            {
                _systems.Add(componentSystem);
                _isCacheDirty = true; // Mark cache as dirty whenever a new system is added
            }
        }

        // NEW: Method to build the eligibility cache
        public void BuildCache(ComponentController controller)
        {
            _systemEligibilityCache.Clear();
            foreach (var system in _systems)
            {
                bool canRun = true;
                foreach (var requiredComponentType in system.RequiredComponents)
                {
                    if (!controller.HasComponent(requiredComponentType))
                    {
                        canRun = false;
                        break;
                    }
                }
                _systemEligibilityCache[system] = canRun;
            }
            _isCacheDirty = false;
        }

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            // Build the cache if it's dirty (e.g., after initialization)
            if (_isCacheDirty)
            {
                BuildCache(controller);
            }

            foreach (var system in _systems)
            {
                // Check 1: Use the cache to see if components are present
                if (!_systemEligibilityCache.TryGetValue(system, out var canRun) || !canRun)
                {
                    continue;
                }

                // Check 2: Check if the system should run on the current side (Server/Client)
                var side = system.ExecutionSide;
                if (side == SystemExecutionSide.Server && Main.netMode == NetmodeID.MultiplayerClient)
                {
                    continue;
                }
                if (side == SystemExecutionSide.Client && Main.netMode == NetmodeID.Server)
                {
                    continue;
                }

                // If all checks pass, update the system
                system.Update(gameTime, npc, controller, eventBus);
            }
        }
    }
}