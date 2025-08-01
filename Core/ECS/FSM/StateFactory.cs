using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.FSM
{
    public class StateFactory
    {
        private readonly Dictionary<Type, Func<IState>> _stateCreationFunctions = new();

        public void RegisterState<T>(Func<T> creationFunc) where T : class, IState
        {
            _stateCreationFunctions[typeof(T)] = () => creationFunc();
        }

        public T CreateState<T>() where T : IState
        {
            if (_stateCreationFunctions.TryGetValue(typeof(T), out var creationFunc))
            {
                return (T)creationFunc();
            }
            throw new ArgumentException($"State of type {typeof(T).Name} is not registered.");
        }
    }
}