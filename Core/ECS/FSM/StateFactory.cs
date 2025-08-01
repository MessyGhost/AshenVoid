using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// A factory responsible for creating and providing state instances on demand.
    /// It caches states after their first creation to avoid repeated instantiation.
    /// </summary>
    public class StateFactory
    {
        private readonly Dictionary<Type, IState> _stateCache = new Dictionary<Type, IState>();

        /// <summary>
        /// Gets or creates an instance of the specified state type.
        /// </summary>
        public T GetState<T>() where T : class, IState, new()
        {
            if (_stateCache.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }

            var newState = new T();
            _stateCache[typeof(T)] = newState;
            return newState;
        }

        /// <summary>
        /// Gets or creates an instance of the specified state type.
        /// This non-generic version is useful when the type is only known at runtime.
        /// </summary>
        public IState GetState(Type stateType)
        {
            if (!typeof(IState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException($"Type {stateType.Name} does not implement IState.", nameof(stateType));
            }

            if (_stateCache.TryGetValue(stateType, out var state))
            {
                return state;
            }

            // This assumes states have a parameterless constructor.
            // For states with dependencies, a proper DI container would be needed here.
            var newState = (IState)Activator.CreateInstance(stateType);
            _stateCache[stateType] = newState;
            return newState;
        }
    }
}