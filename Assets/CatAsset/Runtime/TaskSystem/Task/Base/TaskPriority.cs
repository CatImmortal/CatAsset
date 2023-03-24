namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务优先级
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// 非常低（资源清单检查）
        /// </summary>
        VeryLow = 0,
        
        /// <summary>
        /// 低（资源加载，资源卸载，资源包卸载）
        /// </summary>
        Low = 1,
        
        /// <summary>
        /// 中（更新时下载的资源包）
        /// </summary>
        Middle = 2,
        
        /// <summary>
        /// 高（外部导入的资源清单读取）
        /// </summary>
        Height = 3,
        
        /// <summary>
        /// 非常高（加载时下载的资源包）
        /// </summary>
        VeryHeight = 4,
        
    }
}