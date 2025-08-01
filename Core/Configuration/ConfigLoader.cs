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