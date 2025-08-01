using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    // NEW: Enum to define where the system should run
    public enum SystemExecutionSide
    {
        Both,
        Server,
        Client
    }

    /// <summary>
    /// Marker interface for a system.
    /// </summary>
    public interface ISystem
    {
        // NEW: Property to declare execution side
        SystemExecutionSide ExecutionSide { get; }
    }

    /// <summary>
    /// Defines a system that operates on a set of components.
    /// </summary>
    public interface IComponentSystem : ISystem
    {
        /// <summary>
        /// Gets the set of component types that this system requires to operate.
        /// </summary>
        HashSet<Type> RequiredComponents { get; }

        /// <summary>
        /// The generic update method for all systems.
        /// </summary>
        void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus);
    }

    // ... (Specific system interfaces remain the same)
    public interface IMovementSystem : IComponentSystem { }
    public interface IAttackSystem : IComponentSystem { }
    public interface IAnimationSystem : IComponentSystem { }
    public interface IStatSystem : IComponentSystem { }
    public interface IAIStateSystem : IComponentSystem { }
    public interface IHealthSystem : IComponentSystem { }
}
