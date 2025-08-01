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
        private readonly ServiceLocator _services;
        private readonly Dictionary<Type, IState> _stateCache = new Dictionary<Type, IState>();

        public StateFactory(ServiceLocator services)
        {
            _services = services;
        }

        /// <summary>
        /// Gets or creates an instance of the specified state type.
        /// </summary>
        public T GetState<T>() where T : class, IState, new()
        {
            return (T)GetState(typeof(T));
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

            // States can now be constructed with dependencies via the ServiceLocator if needed,
            // but for now, we'll stick to parameterless constructors.
            // This could be expanded with ActivatorUtilities if a full DI container is integrated.
            var newState = (IState)Activator.CreateInstance(stateType);
            _stateCache[stateType] = newState;
            return newState;
        }
    }
}