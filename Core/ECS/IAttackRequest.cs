namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Represents the AI's desire to perform an attack.
    /// This is a data container, holding no logic itself.
    /// </summary>
    public interface IAttackRequest { }

    public class ShootProjectileRequest : IAttackRequest
    {
        public int ProjectileType;
        public float Speed;
        public int Damage;
        // Further details like target, direction, etc., can be added here.
    }

    public class MeleeDashRequest : IAttackRequest
    {
        public float DashSpeed;
        public int Damage;
    }
}