using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using Microsoft.Xna.Framework;

namespace AshenVoid.Content.NPCs.NightmareCorruption.Intents
{
    // An intent to shoot a projectile. It carries all necessary data.
    public class ShootProjectileIntent : IAttackIntent
    {
        public Vector2 TargetPosition { get; }
        public ProjectileAttack Stats { get; } // Reference to the config object

        public ShootProjectileIntent(Vector2 target, ProjectileAttack stats)
        {
            TargetPosition = target;
            Stats = stats;
        }
    }
}