using System.Collections.Generic;

namespace AshenVoid.Core.ECS.AI
{
    public class AIBlackboardComponent : IComponent
    {
        private readonly Dictionary<BlackboardKey, object> _data = new();

        public void Set<T>(BlackboardKey key, T value)
        {
            _data[key] = value;
        }

        public T Get<T>(BlackboardKey key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public bool Has(BlackboardKey key)
        {
            return _data.ContainsKey(key);
        }
    }
}