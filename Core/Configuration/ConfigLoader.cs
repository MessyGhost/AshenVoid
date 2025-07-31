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
        public T LoadForBoss<T>(string bossFullName) where T : class, new()
        {
            if (_configCache.TryGetValue(bossFullName, out var cached))
                return (T)cached;

            if (string.IsNullOrEmpty(_modSourcePath))
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"由于Mod源路径无效，无法加载Boss配置: {bossFullName}");
                return new T();
            }

            var bossNameParts = bossFullName.Split('/');
            if (bossNameParts.Length < 2)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"无效的Boss FullName格式: {bossFullName}");
                return new T();
            }
            var bossInternalName = bossNameParts[^1];
            var configPath = Path.Combine("Content", "NPCs", bossInternalName, "Configs", $"{bossInternalName}.hjson");

            var fullPath = Path.Combine(_modSourcePath, configPath);

            if (!File.Exists(fullPath))
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn($"Boss配置文件不存在: {fullPath}，将使用默认配置。");
                var defaultConfig = new T();
                _configCache[bossFullName] = defaultConfig;
                return defaultConfig;
            }

            try
            {
                var hjsonText = File.ReadAllText(fullPath);
                var hjsonValue = HjsonValue.Parse(hjsonText);
                var jsonText = hjsonValue.ToString(Stringify.Plain);
                var config = JsonConvert.DeserializeObject<T>(jsonText);

                _configCache[bossFullName] = config;
                return config;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"加载或解析Boss配置失败: {bossFullName} ({configPath})", ex);
                return new T();
            }
        }

        /// <inheritdoc/>
        public void ReloadBossConfig(string bossFullName)
        {
            _configCache.Remove(bossFullName);
        }
    }
}