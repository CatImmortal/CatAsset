namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包更新结果
    /// </summary>
    public readonly struct BundleUpdateResult
    {
        /// <summary>
        /// 是否更新成功
        /// </summary>
        public readonly bool Success;

        /// <summary>
        /// 资源包更新器信息
        /// </summary>
        public readonly UpdateInfo UpdateInfo;
        
        /// <summary>
        /// 此资源包的资源组更新器
        /// </summary>
        public readonly GroupUpdater Updater;

        public BundleUpdateResult(bool success, UpdateInfo updateInfo, GroupUpdater updater)
        {
            Success = success;
            UpdateInfo = updateInfo;
            Updater = updater;
        }
        
        public override string ToString()
        {
            return $"BundleUpdateResult Success:{Success},BundleRelativePath:{UpdateInfo.Info.RelativePath},GroupName:{Updater.GroupName},UpdatedCount:{Updater.UpdatedCount},UpdatedLength:{Updater.UpdatedLength},TotalCount:{Updater.TotalCount},TotalLength:{Updater.TotalLength}";
        }
    }
}