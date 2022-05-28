namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Free,

        /// <summary>
        /// 等待中
        /// </summary>
        Waiting,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 已结束
        /// </summary>
        Finished,
    }

}