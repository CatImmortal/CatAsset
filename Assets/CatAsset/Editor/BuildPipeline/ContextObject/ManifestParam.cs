using CatAsset.Runtime;

namespace CatAsset.Editor
{
    /// <inheritdoc />
    public class ManifestParam : IManifestParam
    {
        /// <inheritdoc />
        public CatAssetManifest Manifest { get; }
        
        /// <inheritdoc />
        public string WritePath { get; }

        public ManifestParam(CatAssetManifest manifest, string writePath)
        {
            Manifest = manifest;
            WritePath = writePath;
        }
        
    }
}