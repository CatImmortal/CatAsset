namespace CatAsset.Runtime
{
    /// <summary>
    /// 加载模式
    /// </summary>
    public enum LoadMode
    {
        /// <summary>
        /// 仅使用安装包内资源（单机模式）
        /// </summary>
        PackageOnly,

        /// <summary>
        /// 可更新模式
        /// </summary>
        Updatable,
    }
}