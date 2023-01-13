namespace CatAsset.Runtime
{
    /// <summary>
    /// 可更新资源加载器
    /// </summary>
    public class UpdatableAssetLoader : DefaultAssetLoader
    {
        /// <inheritdoc />
        public override void CheckVersion(OnVersionChecked onVersionChecked)
        {
            VersionChecker.CheckVersion(onVersionChecked);
        }
    }
}