using System.Collections.Generic;

namespace AshenVoid.Core.ECS.AI
{
    public class AIBlackboardComponent : IComponent
    {
        private readonly Dictionary<string, object> _data = new();

        public void Set<T>(string key, T value)
        {
            _data[key] = value;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public bool Has(string key)
        {
            return _data.ContainsKey(key);
        }
    }
}