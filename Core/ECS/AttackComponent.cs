using AshenVoid.Core.ECS.Intents;
using AshenVoid.Core.ECS.Interfaces;

namespace AshenVoid.Core.ECS
{
    public class AttackComponent : IAttackComponent
    {
        public IAttackIntent CurrentIntent { get; set; }
        public float CooldownTimer { get; set; }

        public AttackComponent()
        {
        }

        public void SetIntent(IAttackIntent intent)
        {
            if (IsReady())
                CurrentIntent = intent;
        }

        public bool IsReady() => CooldownTimer <= 0;
        public bool IsAttacking() => CurrentIntent != null;

        public void Update()
        {
            // All logic is moved to AttackSystem.
        }

        public void Reset()
        {
            CurrentIntent = null;
            CooldownTimer = 0;
        }
    }
}