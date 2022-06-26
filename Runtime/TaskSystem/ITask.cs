namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务接口
    /// </summary>
    public interface ITask : IReference
    {
        /// <summary>
        /// 持有者
        /// </summary>
        TaskRunner Owner { get; }
        
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 状态
        /// </summary>
        TaskState State { get; set; }
        
        /// <summary>
        /// 进度
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// 已合并任务数量
        /// </summary>
        public int MergedTaskCount { get; }
        
        /// <summary>
        /// 合并任务
        /// </summary>
        void MergeTask(ITask task);
        
        /// <summary>
        /// 运行任务
        /// </summary>
        void Run();
        
        /// <summary>
        /// 轮询任务
        /// </summary>
        void Update();
    }
}