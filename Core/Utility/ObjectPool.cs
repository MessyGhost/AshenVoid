using System;
using System.Collections.Generic;

namespace AshenVoid.Core.Utility
{
    /// <summary>
    /// A generic object pool to reduce GC pressure by reusing objects.
    /// Pooled objects should have a parameterless constructor and a public Reset() method.
    /// </summary>
    public static class ObjectPool
    {
        private static readonly Dictionary<Type, Stack<object>> _pools = new Dictionary<Type, Stack<object>>();

        /// <summary>
        /// Gets an object from the pool or creates a new one if the pool is empty.
        /// </summary>
        /// <typeparam name="T">The type of object to get.</typeparam>
        /// <returns>An instance of T.</returns>
        public static T Get<T>() where T : class, new()
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var stack) && stack.Count > 0)
            {
                return (T)stack.Pop();
            }
            return new T();
        }

        /// <summary>
        /// Returns an object to the pool.
        /// The object is expected to be in a "reset" state.
        /// </summary>
        /// <param name="obj">The object to release.</param>
        public static void Release<T>(T obj) where T : class
        {
            if (obj == null)
                return;

            var type = obj.GetType();
            if (!_pools.TryGetValue(type, out var stack))
            {
                stack = new Stack<object>();
                _pools[type] = stack;
            }

            // Attempt to call Reset() method if it exists.
            // This is a soft requirement; objects without Reset() can still be pooled.
            var resetMethod = type.GetMethod("Reset");
            resetMethod?.Invoke(obj, null);

            stack.Push(obj);
        }

        /// <summary>
        /// Clears all object pools.
        /// </summary>
        public static void Clear()
        {
            _pools.Clear();
        }
    }
}