namespace AshenVoid.Core.Configuration
{
    /// <summary>
    /// 定义了配置加载器的契约。
    /// </summary>
    public interface IConfigLoader
    {
        /// <summary>
        /// 根据Boss的FullName加载其配置。
        /// </summary>
        /// <typeparam name="T">配置的类型。</typeparam>
        /// <param name="bossFullName">Boss的FullName属性，例如 "MyMod/MyBoss"。</param>
        /// <returns>加载并反序列化后的配置对象。</returns>
        T LoadForBoss<T>(string bossFullName) where T : class, new();

        /// <summary>
        /// 从缓存中移除一个Boss配置，使其在下次加载时被重新读取。
        /// </summary>
        /// <param name="bossFullName">要重载的Boss的FullName。</param>
        void ReloadBossConfig(string bossFullName);
    }
}