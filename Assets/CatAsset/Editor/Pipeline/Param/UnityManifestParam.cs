using UnityEngine;

namespace CatAsset.Editor
{
    /// <summary>
    /// AssetBundle清单参数
    /// </summary>
    public class UnityManifestParam : IBuildPipelineParam
    {
        public AssetBundleManifest UnityManifest;
    }
}