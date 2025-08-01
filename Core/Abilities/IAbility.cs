using AshenVoid.Core.ECS;
using AshenVoid.Content.NPCs.NightmareCorruption.Configs;

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
        AbilityType Type { get; }
        AbilityStatus Status { get; }

        /// <summary>
        /// Initializes the ability with its configuration.
        /// </summary>
        void Initialize(AbilityConfig config);

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