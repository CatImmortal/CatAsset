using CatAsset.Runtime;

namespace CatAsset.Editor
{
    /// <inheritdoc />
    public class ManifestParam : IManifestParam
    {
        /// <inheritdoc />
        public CatAssetManifest Manifest { get; }
        
        /// <inheritdoc />
        public string WriteFolder { get; }

        public ManifestParam(CatAssetManifest manifest, string writeFolder)
        {
            Manifest = manifest;
            WriteFolder = writeFolder;
        }
        
    }
}