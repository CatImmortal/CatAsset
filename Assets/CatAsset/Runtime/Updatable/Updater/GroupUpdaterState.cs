namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源组更新器状态
    /// </summary>
    public enum GroupUpdaterState
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Free,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 已暂停
        /// </summary>
        Paused,
    }
}