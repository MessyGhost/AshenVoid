using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    #region System Ordering
    
    /// <summary>
    /// Base class for system update groups. Used to define broad execution phases.
    /// </summary>
    public abstract class SystemGroup { }

    /// <summary>
    /// Executes before the main simulation logic. Good for input processing or data setup.
    /// </summary>
    public class InitializationSystemGroup : SystemGroup { }

    /// <summary>
    /// The main update group for core game logic like AI, physics, and state changes.
    /// </summary>
    public class SimulationSystemGroup : SystemGroup { }

    /// <summary>
    /// Executes after the main simulation. Good for cleanup, rendering, or interpolation.
    /// </summary>
    public class PresentationSystemGroup : SystemGroup { }

    /// <summary>
    /// Specifies the update group for a system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UpdateInGroupAttribute : Attribute
    {
        public Type GroupType { get; }
        public UpdateInGroupAttribute(Type groupType)
        {
            if (!typeof(SystemGroup).IsAssignableFrom(groupType))
                throw new ArgumentException("Type must be a subclass of SystemGroup.", nameof(groupType));
            GroupType = groupType;
        }
    }

    /// <summary>
    /// Specifies that the current system must run after another system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class UpdateAfterAttribute : Attribute
    {
        public Type SystemType { get; }
        public UpdateAfterAttribute(Type systemType) => SystemType = systemType;
    }

    /// <summary>
    /// Specifies that the current system must run before another system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class UpdateBeforeAttribute : Attribute
    {
        public Type SystemType { get; }
        public UpdateBeforeAttribute(Type systemType) => SystemType = systemType;
    }

    #endregion

    public enum SystemExecutionSide
    {
        Both,
        Server,
        Client
    }

    public interface ISystem
    {
        SystemExecutionSide ExecutionSide { get; }
    }

    // This is the primary interface for any system that operates on components.
    public interface IComponentSystem : ISystem
    {
        // Defines the set of components an entity must have for this system to run.
        HashSet<Type> RequiredComponents { get; }
        
        // The main update logic for the system.
        void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus);
    }
}