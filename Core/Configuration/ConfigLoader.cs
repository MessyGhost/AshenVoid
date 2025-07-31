using System;
using System.Collections.Generic;
using System.IO;
using Hjson;
using Newtonsoft.Json;
using Terraria.ModLoader;

namespace AshenVoid.Core.Configuration
{
    public static class ConfigLoader
    {
        private static readonly Dictionary<string, object> _configCache = new();

        public static T Load<T>(string configPath) where T : class, new()
        {
            if (_configCache.TryGetValue(configPath, out var cached))
                return (T)cached;

            // 使用在主Mod类中缓存的、通过正确API(SourceFolder)获取的路径
            var modSourcePath = AshenVoid.ModSourcePath;
            if (string.IsNullOrEmpty(modSourcePath))
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error("Mod源路径未初始化，配置加载失败。");
                return new T();
            }

            // 任务文本明确要求路径为 Content/NPCs/NightmareCorruption/Configs/
            var configDir = Path.Combine(modSourcePath, "Content", "NPCs", "NightmareCorruption", "Configs");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            var fullPath = Path.Combine(configDir, configPath);

            if (!File.Exists(fullPath))
            {
                ModContent.GetInstance<AshenVoid>().Logger.Warn($"配置文件不存在: {fullPath}，使用默认配置");
                return new T();
            }

            try
            {
                var hjsonText = File.ReadAllText(fullPath);
                // 绕过Hjson.Net v3的反序列化问题
                // 1. 将Hjson解析为JsonValue对象
                var hjsonValue = HjsonValue.Parse(hjsonText);
                // 2. 将JsonValue对象转换为标准JSON字符串
                var jsonText = hjsonValue.ToString(Stringify.Plain);
                // 3. 使用tModLoader自带的Newtonsoft.Json进行反序列化
                var config = JsonConvert.DeserializeObject<T>(jsonText);

                _configCache[configPath] = config;
                return config;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<AshenVoid>().Logger.Error($"配置加载失败: {configPath}", ex);
                return new T();
            }
        }

        public static void ReloadConfig(string configPath)
        {
            _configCache.Remove(configPath);
        }
    }
}