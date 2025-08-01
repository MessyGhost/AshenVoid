using System.Collections.Generic;

namespace AshenVoid.Core.ECS.AI
{
    public class AIBlackboardComponent : IComponent
    {
        private readonly Dictionary<BlackboardKey, object> _data = new();

        // Timers for various AI actions to reduce processing frequency.
        public float FindTargetCooldown { get; set; } = 0f;
        // Can add more timers here, e.g., public float AbilityCooldown { get; set; } = 0f;

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