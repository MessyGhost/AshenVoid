using System;
using System.Collections.Generic;
using Hjson;
using Newtonsoft.Json;
using Terraria.ModLoader;

namespace AshenVoid.Core.Configuration
{
    public class ConfigLoader : IConfigLoader
    {
        // Singleton instance
        public static ConfigLoader Instance { get; private set; }

        private readonly Dictionary<string, object> _configCache = new();
        private readonly Mod _modInstance;

        // Private constructor for singleton pattern
        private ConfigLoader()
        {
            _modInstance = ModContent.GetInstance<AshenVoid>();
        }

        // Called by the mod to initialize the singleton
        public static void Load()
        {
            Instance = new ConfigLoader();
        }

        // Called by the mod to unload the singleton
        public static void Unload()
        {
            Instance = null;
        }

        /// <summary>
        /// Loads a configuration file for a boss by automatically determining the path from its type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration object.</typeparam>
        /// <param name="npc">The ModNPC instance of the boss.</param>
        /// <returns>The loaded configuration object.</returns>
        public T LoadBossConfig<T>(ModNPC npc) where T : class, new()
        {
            var type = npc.GetType();
            string fullName = type.FullName ?? string.Empty;
            string modName = _modInstance.Name;

            // Example: AshenVoid.Content.NPCs.NightmareCorruption.NightmareCorruption
            // Becomes: Content/NPCs/NightmareCorruption
            string basePath = fullName.Replace(modName + ".", "").Replace('.', '/');

            // Get the class name
            string bossName = type.Name;

            // Final path: Content/NPCs/NightmareCorruption/Configs/NightmareCorruption.hjson
            string assetPath = $"{basePath}/Configs/{bossName}.hjson";

            return Load<T>(assetPath);
        }


        /// <inheritdoc/>
        public T Load<T>(string assetPath) where T : class, new()
        {
            if (_configCache.TryGetValue(assetPath, out var cached))
                return (T)cached;

            if (!_modInstance.FileExists(assetPath))
            {
                _modInstance.Logger.Warn($"Configuration file not found: {assetPath}. Using default config.");
                var defaultConfig = new T();
                _configCache[assetPath] = defaultConfig;
                return defaultConfig;
            }

            try
            {
                var fileBytes = _modInstance.GetFileBytes(assetPath);
                var hjsonText = System.Text.Encoding.UTF8.GetString(fileBytes);

                var hjsonValue = HjsonValue.Parse(hjsonText);
                var jsonText = hjsonValue.ToString(Stringify.Plain);
                var config = JsonConvert.DeserializeObject<T>(jsonText);

                _configCache[assetPath] = config;
                return config;
            }
            catch (Exception ex)
            {
                _modInstance.Logger.Error($"Failed to load or parse config: {assetPath}", ex);
                return new T(); // Return default on failure
            }
        }

        /// <inheritdoc/>
        public void ReloadConfig(string assetPath)
        {
            _configCache.Remove(assetPath);
        }
    }
}