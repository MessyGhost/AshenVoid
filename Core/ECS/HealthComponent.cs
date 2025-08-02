namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Stores health-related state for an entity.
    /// Using a struct to avoid heap allocation for this small component.
    /// </summary>
    public struct HealthComponent : IComponent
    {
        public int CurrentHealth;
        public int MaxHealth;
    }
}