using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using AshenVoid.Core.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<IComponentSystem> _registeredSystems = new List<IComponentSystem>();
        private List<IComponentSystem> _sortedSystems = new List<IComponentSystem>();

        private readonly Dictionary<IComponentSystem, bool> _systemEligibilityCache = new Dictionary<IComponentSystem, bool>();
        private bool _isCacheDirty = true;

        public void RegisterSystem(ISystem system)
        {
            if (system is IComponentSystem componentSystem)
            {
                _registeredSystems.Add(componentSystem);
                _isCacheDirty = true;
            }
        }

        public void BuildCache(ComponentController controller)
        {
            if (!_isCacheDirty) return;

            // Step 1: Sort systems based on dependencies
            try
            {
                _sortedSystems = SortSystems(_registeredSystems);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"Failed to sort systems: {ex.Message}");
                // Fallback to registration order to prevent a crash
                _sortedSystems = new List<IComponentSystem>(_registeredSystems);
            }

            // Step 2: Build eligibility cache based on sorted list
            _systemEligibilityCache.Clear();
            foreach (var system in _sortedSystems)
            {
                bool canRun = system.RequiredComponents.All(controller.HasComponent);
                _systemEligibilityCache[system] = canRun;
            }

            _isCacheDirty = false;
        }

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            if (_isCacheDirty)
            {
                BuildCache(controller);
            }

            foreach (var system in _sortedSystems)
            {
                if (!_systemEligibilityCache.TryGetValue(system, out var canRun) || !canRun)
                {
                    continue;
                }

                var side = system.ExecutionSide;
                if ((side == SystemExecutionSide.Server && Main.netMode == NetmodeID.MultiplayerClient) ||
                    (side == SystemExecutionSide.Client && Main.netMode == NetmodeID.Server))
                {
                    continue;
                }

                system.Update(gameTime, npc, controller, eventBus);
            }
        }

        private List<IComponentSystem> SortSystems(List<IComponentSystem> systems)
        {
            var systemTypes = systems.ToDictionary(s => s.GetType(), s => s);
            var graph = new Dictionary<Type, List<Type>>();
            var inDegree = systems.ToDictionary(s => s.GetType(), s => 0);

            // Build the dependency graph
            foreach (var system in systems)
            {
                var systemType = system.GetType();
                if (!graph.ContainsKey(systemType)) graph[systemType] = new List<Type>();

                var afterDeps = system.GetType().GetCustomAttributes<UpdateAfterAttribute>();
                foreach (var dep in afterDeps)
                {
                    if (systemTypes.ContainsKey(dep.SystemType))
                    {
                        graph[dep.SystemType].Add(systemType);
                        inDegree[systemType]++;
                    }
                }

                var beforeDeps = system.GetType().GetCustomAttributes<UpdateBeforeAttribute>();
                foreach (var dep in beforeDeps)
                {
                    if (systemTypes.ContainsKey(dep.SystemType))
                    {
                        graph[systemType].Add(dep.SystemType);
                        inDegree[dep.SystemType]++;
                    }
                }
            }

            // Group systems
            var groups = systems.GroupBy(s =>
            {
                var attr = s.GetType().GetCustomAttribute<UpdateInGroupAttribute>();
                return attr?.GroupType; // Can be null for systems without a group
            }).ToDictionary(g => g.Key, g => g.ToList());

            var sortedList = new List<IComponentSystem>();
            var groupOrder = new List<Type> { typeof(InitializationSystemGroup), typeof(SimulationSystemGroup), typeof(PresentationSystemGroup), null };

            foreach (var groupType in groupOrder)
            {
                if (!groups.TryGetValue(groupType, out var groupSystems)) continue;

                var queue = new Queue<Type>(groupSystems.Select(s => s.GetType()).Where(t => inDegree[t] == 0));
                var groupSorted = new List<IComponentSystem>();

                while (queue.Count > 0)
                {
                    var currentType = queue.Dequeue();
                    groupSorted.Add(systemTypes[currentType]);

                    if (graph.ContainsKey(currentType))
                    {
                        foreach (var neighbor in graph[currentType])
                        {
                            if (groupSystems.Any(s => s.GetType() == neighbor)) // Ensure dependency is within the same group
                            {
                                inDegree[neighbor]--;
                                if (inDegree[neighbor] == 0)
                                {
                                    queue.Enqueue(neighbor);
                                }
                            }
                        }
                    }
                }

                if (groupSorted.Count != groupSystems.Count)
                {
                    var unsorted = groupSystems.Where(s => !groupSorted.Contains(s)).Select(s => s.GetType().Name);
                    throw new Exception($"Circular dependency detected in system group {groupType?.Name ?? "Default"}. Unsorted systems: {string.Join(", ", unsorted)}");
                }
                
                sortedList.AddRange(groupSorted);
            }

            return sortedList;
        }
    }
}