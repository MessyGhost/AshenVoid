using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    /// <summary>
    /// Marker interface for a system.
    /// </summary>
    public interface ISystem { }

    /// <summary>
    /// Defines a system that operates on a set of components.
    /// This is the core of the new, efficient SystemManager.
    /// </summary>
    public interface IComponentSystem : ISystem
    {
        /// <summary>
        /// Gets the set of component types that this system requires to operate.
        /// The SystemManager will use this to determine if the system should run for a given entity.
        /// </summary>
        HashSet<Type> RequiredComponents { get; }

        /// <summary>
        /// The generic update method for all systems.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="npc">The NPC entity.</param>
        /// <param name="controller">The component controller for accessing components.</param>
        /// <param name="eventBus">The event bus for publishing events.</param>
        void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus);
    }

    // The specific system interfaces are now just for categorization and clarity.
    // They no longer define their own Update methods.
    // This simplifies the SystemManager immensely.

    public interface IMovementSystem : IComponentSystem { }
    public interface IAttackSystem : IComponentSystem { }
    public interface IAnimationSystem : IComponentSystem { }
    public interface IStatSystem : IComponentSystem { }
    public interface IAIStateSystem : IComponentSystem { }
    public interface IHealthSystem : IComponentSystem { }
}
