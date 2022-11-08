namespace CatAsset.Runtime
{
    /// <summary>
    /// 句柄状态
    /// </summary>
    public enum HandlerState
    {
        /// <summary>
        /// 无效
        /// </summary>
        InValid,
        
        /// <summary>
        /// 加载中
        /// </summary>
        Doing,
        
        /// <summary>
        /// 加载成功
        /// </summary>
        Success,
        
        /// <summary>
        /// 加载失败
        /// </summary>
        Failed,
    }
}