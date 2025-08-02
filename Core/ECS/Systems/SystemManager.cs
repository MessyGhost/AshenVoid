using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly List<EntityQuerySystem> _parallelSystems = new();
        private readonly List<EntityQuerySystem> _sequentialSystems = new();

        public void RegisterSystem(ISystem system)
        {
            _systems.Add(system);
            if (system is EntityQuerySystem querySystem)
            {
                if (querySystem.GetType().GetCustomAttribute<ParallelizableAttribute>() != null)
                {
                    _parallelSystems.Add(querySystem);
                }
                else
                {
                    _sequentialSystems.Add(querySystem);
                }
            }
        }

        public void Update(GameTime gameTime, EcsWorld world, EventBus eventBus)
        {
            var tasks = new List<Task>();

            // Start parallel systems
            foreach (var system in _parallelSystems)
            {
                if (ShouldRun(system))
                {
                    tasks.Add(Task.Run(() => system.UpdateAll(gameTime, world, eventBus)));
                }
            }

            // Run sequential systems on the main thread
            foreach (var system in _sequentialSystems)
            {
                if (ShouldRun(system))
                {
                    system.UpdateAll(gameTime, world, eventBus);
                }
            }

            // Wait for parallel systems to complete
            if (tasks.Any())
            {
                Task.WaitAll(tasks.ToArray());
            }
        }

        private bool ShouldRun(EntityQuerySystem querySystem)
        {
            var side = querySystem.ExecutionSide;
            if ((side == SystemExecutionSide.Server && Main.netMode == NetmodeID.MultiplayerClient) ||
                (side == SystemExecutionSide.Client && Main.netMode == NetmodeID.Server))
            {
                return false;
            }
            return true;
        }
    }
}