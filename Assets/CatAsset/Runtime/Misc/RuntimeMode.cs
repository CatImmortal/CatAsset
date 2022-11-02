namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源管理器运行模式
    /// </summary>
    public enum RuntimeMode
    {
        /// <summary>
        /// 仅使用安装包内资源（WebGL平台只能使用此模式）
        /// </summary>
        PackageOnly,

        /// <summary>
        /// 可更新模式
        /// </summary>
        Updatable,
    }
}