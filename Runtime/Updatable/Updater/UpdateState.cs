namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包更新状态
    /// </summary>
    public enum UpdateState
    {
        None,
        
        /// <summary>
        /// 待更新
        /// </summary>
        Waiting,
        
        /// <summary>
        /// 更新中
        /// </summary>
        Updating,
        
        /// <summary>
        /// 已更新
        /// </summary>
        Updated,
    }
}