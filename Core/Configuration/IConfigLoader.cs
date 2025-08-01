namespace AshenVoid.Core.Configuration
{
    /// <summary>
    /// Defines the contract for the configuration loader.
    /// </summary>
    public interface IConfigLoader
    {
        /// <summary>
        /// Loads a configuration file from the mod's content.
        /// </summary>
        /// <typeparam name="T">The type of the configuration object.</typeparam>
        /// <param name="assetPath">The relative path to the asset within the mod (e.g., "Content/NPCs/MyBoss/Configs/MyBoss.hjson").</param>
        /// <returns>The loaded and deserialized configuration object, or a default object on failure.</returns>
        T Load<T>(string assetPath) where T : class, new();

        /// <summary>
        /// Removes a configuration from the cache, forcing a reload on next access.
        /// </summary>
        /// <param name="assetPath">The asset path of the configuration to reload.</param>
        void ReloadConfig(string assetPath);
    }
}