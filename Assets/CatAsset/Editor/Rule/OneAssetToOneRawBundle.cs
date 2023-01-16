namespace CatAsset.Editor
{
    /// <summary>
    /// 将单个资源构建为单个原生资源包
    /// </summary>
    public class OneAssetToOneRawBundle : OneAssetToOneBundle
    {
        /// <inheritdoc />
        public override bool IsRaw => true;
    }
}