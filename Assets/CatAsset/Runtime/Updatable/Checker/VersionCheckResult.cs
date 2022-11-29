using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 版本检查结果
    /// </summary>
    public readonly struct VersionCheckResult
    {
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

        public VersionCheckResult(string error,int totalCount, ulong totalLength)
        {
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
            return $"VersionCheckResult Error:{Error ?? "null"},TotalCount:{TotalCount},TotalLength:{TotalLength}";
        }
    }
}