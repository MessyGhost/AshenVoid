using System;
using System.Collections.Generic;
using System.IO;
using Hjson;
using Newtonsoft.Json;
using Terraria.ModLoader;

namespace AshenVoid.Core.Configuration
{
    /// <summary>
    /// 一个通用的、可注入的配置加载器，用于从.hjson文件加载配置。
    /// </summary>
    public class ConfigLoader : IConfigLoader
    {
        private readonly Dictionary<string, object> _configCache = new();
        private readonly string _modSourcePath;

        public ConfigLoader()
        {
            _modSourcePath = AshenVoid.ModSourcePath;
            if (string.IsNullOrEmpty(_modSourcePath))
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error("Mod源路径未初始化，配置加载器可能无法正常工作。");
            }
        }

        /// <inheritdoc/>
        public T Load<T>(string configPath) where T : class, new()
        {
            if (_configCache.TryGetValue(configPath, out var cached))
                return (T)cached;

            if (string.IsNullOrEmpty(_modSourcePath))
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"由于Mod源路径无效，无法加载配置: {configPath}");
                return new T();
            }

            var fullPath = Path.Combine(_modSourcePath, configPath);

            if (!File.Exists(fullPath))
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn($"配置文件不存在: {fullPath}，将使用默认配置。");
                var defaultConfig = new T();
                // 将默认配置也缓存起来，避免重复的文件检查
                _configCache[configPath] = defaultConfig;
                return defaultConfig;
            }

            try
            {
                var hjsonText = File.ReadAllText(fullPath);
                var hjsonValue = HjsonValue.Parse(hjsonText);
                var jsonText = hjsonValue.ToString(Stringify.Plain);
                var config = JsonConvert.DeserializeObject<T>(jsonText);

                _configCache[configPath] = config;
                return config;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"加载或解析配置失败: {configPath}", ex);
                return new T();
            }
        }

        /// <inheritdoc/>
        public void ReloadConfig(string configPath)
        {
            _configCache.Remove(configPath);
        }
    }
}