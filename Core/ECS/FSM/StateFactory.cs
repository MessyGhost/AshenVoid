using System;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS.FSM
{
    public class StateFactory
    {
        private readonly Dictionary<Type, Func<IState>> _stateCreationFunctions = new();
        private readonly Dictionary<Type, byte> _typeToId = new();
        private readonly Dictionary<byte, Type> _idToType = new();
        private byte _nextStateId = 0;

        public void RegisterState<T>(Func<T> creationFunc) where T : class, IState
        {
            var type = typeof(T);
            _stateCreationFunctions[type] = () => creationFunc();

            if (!_typeToId.ContainsKey(type))
            {
                var id = _nextStateId++;
                _typeToId[type] = id;
                _idToType[id] = type;
            }
        }

        public T CreateState<T>() where T : IState
        {
            if (_stateCreationFunctions.TryGetValue(typeof(T), out var creationFunc))
            {
                return (T)creationFunc();
            }
            throw new ArgumentException($"State of type {typeof(T).Name} is not registered.");
        }

        public IState CreateState(Type stateType)
        {
            if (_stateCreationFunctions.TryGetValue(stateType, out var creationFunc))
            {
                return creationFunc();
            }
            throw new ArgumentException($"State of type {stateType.Name} is not registered.");
        }

        public byte GetIdByType(Type stateType)
        {
            if (_typeToId.TryGetValue(stateType, out var id))
            {
                return id;
            }
            throw new ArgumentException($"State type {stateType.Name} is not registered with an ID.");
        }

        public Type GetTypeById(byte id)
        {
            if (_idToType.TryGetValue(id, out var type))
            {
                return type;
            }
            throw new ArgumentException($"State ID {id} is not a registered state.");
        }
    }
}