using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 版本检查结果
    /// </summary>
    public readonly struct VersionCheckResult
    {
        /// <summary>
        /// 是否检查成功
        /// </summary>
        public readonly bool Success;
        
        /// <summary>
        /// 检查失败时的异常信息
        /// </summary>
        public readonly string Error;
        
        /// <summary>
        /// 需要更新的资源包总数量
        /// </summary>
        public readonly int TotalCount;
            
        /// <summary>
        /// 需要更新的资源包总长度
        /// </summary>
        public readonly ulong TotalLength;

        /// <summary>
        /// 资源组更新器列表
        /// </summary>
        public readonly List<GroupUpdater> GroupUpdaters;

        public VersionCheckResult(bool success, string error,int totalCount, ulong totalLength)
        {
            Success = success;
            Error = error;
            TotalCount = totalCount;
            TotalLength = totalLength;
            GroupUpdaters = new List<GroupUpdater>(CatAssetUpdater.GroupUpdaterDict.Count);
            foreach (var pair in CatAssetUpdater.GroupUpdaterDict)
            {
                GroupUpdaters.Add(pair.Value);
            }
        }

        public override string ToString()
        {
            return $"VersionCheckResult Success:{Success} Error:{Error ?? "null"},TotalCount:{TotalCount},TotalLength:{TotalLength}";
        }
    }
}