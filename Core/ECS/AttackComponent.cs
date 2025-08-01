namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// A component to hold data related to attacking.
    /// In the new architecture, this is a pure data container.
    /// </summary>
    public class AttackComponent : IComponent
    {
        // Attack-related data would go here, for example:
        public float AttackCooldown { get; set; }
        public float LastAttackTime { get; set; }
    }
}