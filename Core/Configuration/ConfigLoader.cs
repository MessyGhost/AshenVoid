using System;
using System.Collections.Generic;
using Hjson;
using Newtonsoft.Json;
using Terraria.ModLoader;

namespace AshenVoid.Core.Configuration
{
    public class ConfigLoader : IConfigLoader
    {
        private readonly Dictionary<string, object> _configCache = new();
        private readonly Mod _modInstance;

        public ConfigLoader()
        {
            // Store the Mod instance to access its file loading capabilities
            _modInstance = ModContent.GetInstance<AshenVoid>();
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
                // Use Mod.GetFileBytes to read the file safely in both dev and published environments
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