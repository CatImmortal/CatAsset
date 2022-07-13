using CatAsset.Runtime;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// CatAsset资源清单参数
    /// </summary>
    public interface IManifestParam : IContextObject
    {
        public CatAssetManifest Manifest { get; }
        public string WritePath { get;}
    }
}