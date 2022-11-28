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
        public ulong TotalLength;

        public VersionCheckResult(string error,int totalCount, ulong totalLength)
        {
            Error = error;
            TotalCount = totalCount;
            TotalLength = totalLength;
        }

        public override string ToString()
        {
            return $"VersionCheckResult Error:{Error ?? "null"},TotalCount:{TotalCount},TotalLength:{TotalLength}";
        }
    }
}