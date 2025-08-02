using System;
using System.Collections.Generic;
using System.Threading;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Manages unique integer IDs for component types.
    /// This is crucial for creating efficient bitmask signatures for archetypes.
    /// </summary>
    public static class ComponentTypeManager
    {
        private static readonly Dictionary<Type, int> s_typeToIndex = new();
        private static int s_nextTypeId = 0;

        /// <summary>
        /// Gets the unique integer ID for a given component type.
        /// If the type is not yet registered, it will be assigned a new ID.
        /// </summary>
        public static int GetId(Type type)
        {
            if (!typeof(IComponent).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type must implement IComponent.", nameof(type));
            }

            lock (s_typeToIndex)
            {
                if (s_typeToIndex.TryGetValue(type, out var id))
                {
                    return id;
                }

                id = Interlocked.Increment(ref s_nextTypeId) - 1;
                s_typeToIndex[type] = id;
                return id;
            }
        }

        public static int GetTotalComponentTypes() => s_nextTypeId;
    }
}