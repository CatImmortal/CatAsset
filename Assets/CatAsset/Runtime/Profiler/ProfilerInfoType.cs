namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器信息类型
    /// </summary>
    public enum ProfilerInfoType
    {
        None,

        /// <summary>
        /// 资源包信息
        /// </summary>
        Bundle,

        /// <summary>
        /// 任务信息
        /// </summary>
        Task,

        /// <summary>
        /// 资源组信息
        /// </summary>
        Group,

        /// <summary>
        /// 更新器信息
        /// </summary>
        Updater,
    }
}
