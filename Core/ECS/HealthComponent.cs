namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Stores health-related state for an entity.
    /// </summary>
    public class HealthComponent : IComponent
    {
        public int CurrentHealth;
        public int MaxHealth;

        public HealthComponent(int initialHealth, int maxHealth)
        {
            CurrentHealth = initialHealth;
            MaxHealth = maxHealth;
        }
    }
}