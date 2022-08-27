namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包更新结果
    /// </summary>
    public struct BundleUpdateResult
    {
        /// <summary>
        /// 是否更新成功
        /// </summary>
        public bool Success;
        
        /// <summary>
        /// 资源包相对路径
        /// </summary>
        public string BundleRelativePath;
        
        /// <summary>
        /// 此资源包的资源组更新器
        /// </summary>
        public GroupUpdater Updater;

        public BundleUpdateResult(bool success, string bundleRelativePath, GroupUpdater updater)
        {
            Success = success;
            BundleRelativePath = bundleRelativePath;
            Updater = updater;
        }
        
        public override string ToString()
        {
            return $"BundleUpdateResult Success:{Success},BundleRelativePath:{BundleRelativePath},GroupName:{Updater.GroupName},UpdatedCount:{Updater.UpdatedCount},UpdatedLength:{Updater.UpdatedLength},TotalCount:{Updater.TotalCount},TotalLength:{Updater.TotalLength}";
        }
    }
}