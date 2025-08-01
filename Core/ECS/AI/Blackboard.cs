using System.Collections.Generic;

namespace AshenVoid.Core.ECS.AI
{
    /// <summary>
    /// A simple key-value store for sharing data between AI components (States, Behavior Tree Nodes, etc.).
    /// This acts as a central "memory" for an AI entity, preventing the need to pass context through method parameters.
    /// </summary>
    public class Blackboard
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        /// <summary>
        /// Stores or updates a value in the blackboard.
        /// </summary>
        public void Set<T>(string key, T value)
        {
            _data[key] = value;
        }

        /// <summary>
        /// Retrieves a value from the blackboard.
        /// </summary>
        /// <returns>The value if found, otherwise the default value for the type (e.g., null for objects).</returns>
        public T Get<T>(string key)
        {
            if (_data.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        /// <summary>
        /// Checks if a key exists in the blackboard.
        /// </summary>
        public bool Has(string key)
        {
            return _data.ContainsKey(key);
        }

        /// <summary>
        /// Tries to retrieve a value from the blackboard.
        /// </summary>
        /// <returns>True if the key was found and the value is of the correct type, otherwise false.</returns>
        public bool TryGet<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out var obj) && obj is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Removes a value from the blackboard.
        /// </summary>
        public void Remove(string key)
        {
            _data.Remove(key);
        }
    }
}