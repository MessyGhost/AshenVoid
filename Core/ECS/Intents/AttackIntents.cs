using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using Microsoft.Xna.Framework;

namespace AshenVoid.Core.ECS.Intents
{
    /// <summary>
    /// Represents the AI's desire to perform an attack.
    /// This is a data container, holding no logic itself.
    /// </summary>
    public interface IAttackIntent { }

    /// <summary>
    /// An intent to shoot a projectile. It carries all necessary data.
    /// </summary>
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