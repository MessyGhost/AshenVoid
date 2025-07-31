using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.FSM
{
    /// <summary>
    /// A factory responsible for creating and providing state instances.
    /// This allows states to be decoupled from each other, as they can request a new state by type
    /// instead of instantiating it directly with `new`.
    /// </summary>
    public class StateFactory
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        public StateFactory(IEnumerable<IState> states)
        {
            foreach (var state in states)
            {
                _states[state.GetType()] = state;
            }
        }

        public T GetState<T>() where T : IState
        {
            if (_states.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }
            throw new ArgumentException($"State of type {typeof(T).Name} is not registered in the factory.");
        }
    }
}