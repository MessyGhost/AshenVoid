namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Stores health-related state for an entity, such as its last known health value.
    /// </summary>
    public class HealthComponent : IComponent
    {
        public int LastHealth;

        public HealthComponent(int initialHealth)
        {
            LastHealth = initialHealth;
        }
    }
}