namespace AshenVoid.Core.Configuration
{
    /// <summary>
    /// 定义了配置加载器的契约。
    /// </summary>
    public interface IConfigLoader
    {
        /// <summary>
        /// 从指定的相对路径加载配置。
        /// </summary>
        /// <typeparam name="T">配置的类型。</typeparam>
        /// <param name="configPath">相对于Mod源目录的配置文件路径。</param>
        /// <returns>加载并反序列化后的配置对象。</returns>
        T Load<T>(string configPath) where T : class, new();

        /// <summary>
        /// 从缓存中移除一个配置，使其在下次加载时被重新读取。
        /// </summary>
        /// <param name="configPath">要重载的配置路径。</param>
        void ReloadConfig(string configPath);
    }
}