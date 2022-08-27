using CatAsset.Runtime;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// CatAsset资源清单参数
    /// </summary>
    public interface IManifestParam : IContextObject
    {
        /// <summary>
        /// CatAsset资源清单
        /// </summary>
        public CatAssetManifest Manifest { get; }
        
        /// <summary>
        /// 写入资源清单的路径
        /// </summary>
        public string WritePath { get;}
    }
}