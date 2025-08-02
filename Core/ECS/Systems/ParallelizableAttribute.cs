using System;

namespace AshenVoid.Core.ECS.Systems
{
    /// <summary>
    /// Marks an EntityQuerySystem as safe to be run in parallel with other systems
    /// that also have this attribute.
    /// The system must not have mutable dependencies on other systems and should not
    /// directly interact with non-thread-safe APIs (like most of Terraria's).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ParallelizableAttribute : Attribute
    {
    }
}