using AshenVoid.Core.ECS;

namespace AshenVoid.Core.Abilities
{
    public enum AbilityStatus
    {
        Idle,
        Charging,
        Active,
        Cooldown
    }

    public interface IAbility
    {
        string Name { get; }
        AbilityStatus Status { get; }

        /// <summary>
        /// Checks if the ability can be used (e.g., cooldown, resources, target range).
        /// </summary>
        bool CanUse(int entityId, EcsWorld world);

        /// <summary>
        /// Starts the ability, potentially entering a charging or active state.
        /// </summary>
        void Start(int entityId, EcsWorld world);

        /// <summary>
        /// Updates the ability's logic while it's active or charging.
        /// </summary>
        void Update(float deltaTime, int entityId, EcsWorld world);

        /// <summary>
        /// Ends the ability, cleaning up and starting the cooldown.
        /// </summary>
        void End(int entityId, EcsWorld world);
    }
}