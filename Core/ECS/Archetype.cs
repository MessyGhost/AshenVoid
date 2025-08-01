using System;
using System.Collections.Generic;
using System.Linq;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Represents a unique combination of component types.
    /// Entities with the same set of components belong to the same archetype.
    /// </summary>
    public class Archetype
    {
        public readonly HashSet<Type> ComponentTypes;
        public readonly List<int> Entities = new List<int>();

        public Archetype(HashSet<Type> componentTypes)
        {
            ComponentTypes = new HashSet<Type>(componentTypes);
        }

        /// <summary>
        /// Checks if this archetype contains all the required component types for a system.
        /// </summary>
        public bool Matches(IEnumerable<Type> requiredComponents)
        {
            // This check is the core of the performance improvement.
            // A system only runs on archetypes that contain all its required components.
            return requiredComponents.All(req => ComponentTypes.Contains(req));
        }
    }
}