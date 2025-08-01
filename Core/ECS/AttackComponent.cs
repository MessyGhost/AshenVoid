using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AttackComponent : IComponent
    {
        private readonly Dictionary<AttackType, float> _attackCooldowns = new();
        private readonly List<AttackType> _keys = new(); // Reusable list for keys

        public AttackComponent()
        {
            // The constructor is now parameterless. Cooldowns are added dynamically.
        }

        public bool CanAttack(AttackType attackType)
        {
            return !_attackCooldowns.TryGetValue(attackType, out var cooldown) || cooldown <= 0;
        }

        public void UseAttack(AttackType attackType, float cooldown)
        {
            _attackCooldowns[attackType] = cooldown;
        }

        public void UpdateCooldowns(float deltaTime)
        {
            // This needs to be called by a system (e.g., AttackSystem)
            _keys.Clear();
            _keys.AddRange(_attackCooldowns.Keys);

            foreach (var key in _keys)
            {
                if (_attackCooldowns[key] > 0)
                {
                    _attackCooldowns[key] -= deltaTime;
                }
            }
        }
    }
}