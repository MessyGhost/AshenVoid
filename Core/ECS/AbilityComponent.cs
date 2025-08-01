using AshenVoid.Core.Abilities;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS
{
    public class AbilityComponent : IComponent
    {
        public readonly Dictionary<string, IAbility> Abilities = new();
        public IAbility ActiveAbility { get; private set; }

        public void AddAbility(IAbility ability)
        {
            Abilities[ability.Name] = ability;
        }

        public IAbility GetAbility(string name)
        {
            Abilities.TryGetValue(name, out var ability);
            return ability;
        }

        public void SetActiveAbility(IAbility ability)
        {
            ActiveAbility = ability;
        }
    }
}