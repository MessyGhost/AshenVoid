using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using System;
using System.Collections.Generic;

namespace AshenVoid.Core.Abilities
{
    public class AbilityFactory
    {
        private readonly Dictionary<string, Func<AbilityConfig, IAbility>> _abilityCreators = new();

        public AbilityFactory()
        {
            // Register concrete ability types here
            // Example:
            // RegisterAbility<DashAbility>("Dash");
        }

        public void RegisterAbility<T>(string typeName) where T : IAbility, new()
        {
            _abilityCreators[typeName] = (config) =>
            {
                // This is a simplified creation process.
                // A more robust solution would use reflection or a proper deserializer
                // to map config.Parameters to the properties of the ability instance.
                var ability = new T();
                // (ability as IConfigurableAbility)?.Configure(config.Parameters);
                return ability;
            };
        }

        public IAbility CreateAbility(AbilityConfig config)
        {
            if (_abilityCreators.TryGetValue(config.Type, out var createFunc))
            {
                return createFunc(config);
            }
            throw new ArgumentException($"Unknown ability type: {config.Type}");
        }
    }
}