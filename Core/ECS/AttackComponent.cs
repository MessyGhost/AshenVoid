using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AttackComponent : IComponent
    {
        private readonly Dictionary<string, float> _attackCooldowns = new();

        public AttackComponent()
        {
            // The constructor is now parameterless. Cooldowns are added dynamically.
        }

        public bool CanAttack(string attackName)
        {
            return !_attackCooldowns.TryGetValue(attackName, out var cooldown) || cooldown <= 0;
        }

        public void UseAttack(string attackName, float cooldown)
        {
            _attackCooldowns[attackName] = cooldown;
        }

        public void UpdateCooldowns(float deltaTime)
        {
            // This needs to be called by a system (e.g., AttackSystem)
            var keys = new List<string>(_attackCooldowns.Keys);
            foreach (var key in keys)
            {
                if (_attackCooldowns[key] > 0)
                {
                    _attackCooldowns[key] -= deltaTime;
                }
            }
        }
    }
}