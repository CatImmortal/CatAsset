using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Pipeline.Interfaces;

namespace CatAsset.Editor
{
    /// <summary>
    /// 资源包构建信息参数
    /// </summary>
    public interface IBundleBuildInfoParam : IContextObject
    {
        /// <summary>
        /// 要构建的AssetBundleBuild列表
        /// </summary>
        List<AssetBundleBuild> AssetBundleBuilds { get; }
        
        /// <summary>
        /// 要构建的普通资源包列表
        /// </summary>
        List<BundleBuildInfo> NormalBundleBuilds { get; }
        
        /// <summary>
        /// 要构建的原生资源包列表
        /// </summary>
        List<BundleBuildInfo> RawBundleBuilds { get; }
    }
}