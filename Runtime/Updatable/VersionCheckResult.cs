namespace CatAsset.Runtime
{
    /// <summary>
    /// 版本检查结果
    /// </summary>
    public struct VersionCheckResult
    {
        /// <summary>
        /// 检查失败时的异常信息
        /// </summary>
        public string Error;
        
        /// <summary>
        /// 需要更新的资源包总数量
        /// </summary>
        public int TotalCount;
            
        /// <summary>
        /// 需要更新的资源包总长度
        /// </summary>
        public long TotalLength;

        public VersionCheckResult(string error,int totalCount, long totalLength)
        {
            Error = error;
            TotalCount = totalCount;
            TotalLength = totalLength;
        }
    }
}