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
        /// 执行中
        /// </summary>
        Doing,
        
        /// <summary>
        /// 执行成功
        /// </summary>
        Success,
        
        /// <summary>
        /// 执行失败
        /// </summary>
        Failed,
    }
}