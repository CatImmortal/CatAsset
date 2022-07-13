using CatAsset.Runtime;

namespace CatAsset.Editor
{
    /// <inheritdoc />
    public class ManifestParam : IManifestParam
    {
        public CatAssetManifest Manifest { get; }
        public string WritePath { get; }

        public ManifestParam(CatAssetManifest manifest, string writePath)
        {
            Manifest = manifest;
            WritePath = writePath;
        }
        
        
    }
}