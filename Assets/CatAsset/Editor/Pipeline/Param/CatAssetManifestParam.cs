using CatAsset.Runtime;

namespace CatAsset.Editor
{
    /// <summary>
    /// CatAsset资源清单参数
    /// </summary>
    public class CatAssetManifestParam : IBuildPipelineParam
    {
        public CatAssetManifest Manifest;
    }
}