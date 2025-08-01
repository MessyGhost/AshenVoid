using AshenVoid.Core.Abilities;
using System.Collections.Generic;

namespace AshenVoid.Core.ECS
{
    public class AbilityComponent : IComponent
    {
        public readonly Dictionary<AbilityType, IAbility> Abilities = new();
        public IAbility ActiveAbility { get; private set; }

        public void AddAbility(IAbility ability)
        {
            Abilities[ability.Type] = ability;
        }

        public IAbility GetAbility(AbilityType type)
        {
            Abilities.TryGetValue(type, out var ability);
            return ability;
        }

        public void SetActiveAbility(IAbility ability)
        {
            ActiveAbility = ability;
        }
    }
}