using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AttackComponent : IComponent
    {
        public readonly float[] _attackCooldowns;

        public AttackComponent(int numAttacks = 1)
        {
            _attackCooldowns = new float[numAttacks];
        }

        public bool CanAttack(int attackId)
        {
            return attackId >= 0 && attackId < _attackCooldowns.Length && _attackCooldowns[attackId] <= 0;
        }

        public void UseAttack(int attackId, float cooldown)
        {
            if (attackId >= 0 && attackId < _attackCooldowns.Length)
            {
                _attackCooldowns[attackId] = cooldown;
            }
        }
    }
}