using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包运行时信息
    /// </summary>
    public class BundleRuntimeInfo : IComparable<BundleRuntimeInfo>,IEquatable<BundleRuntimeInfo>
    {
        /// <summary>
        /// 资源包清单信息
        /// </summary>
        public BundleManifestInfo Manifest;

        /// <summary>
        /// 资源包实例
        /// </summary>
        public AssetBundle Bundle;

        /// <summary>
        /// 是否位于读写区
        /// </summary>
        public bool InReadWrite;

        private string loadPath;
        /// <summary>
        /// 加载地址
        /// </summary>
        public string LoadPath
        {
            get
            {
                if (loadPath == null)
                {
                    if (InReadWrite)
                    {
                        loadPath = Util.GetReadWritePath(Manifest.RelativePath);
                    }
                    else
                    {
                        loadPath = Util.GetReadOnlyPath(Manifest.RelativePath);
                    }
                }
                return loadPath;
            }
        }

        /// <summary>
        /// 当前使用中的资源集合，这里面的资源的引用计数都大于0
        /// </summary>
        public HashSet<AssetRuntimeInfo> UsedAssets { get; } = new HashSet<AssetRuntimeInfo>();

        /// <summary>
        /// 依赖此资源包的资源包集合
        /// </summary>
        public HashSet<BundleRuntimeInfo> RefBundles{ get; } = new HashSet<BundleRuntimeInfo>();
        
        /// <summary>
        /// 此资源包依赖的资源包集合
        /// </summary>
        public HashSet<BundleRuntimeInfo> DependencyBundles { get; } = new HashSet<BundleRuntimeInfo>();
        
        /// <summary>
        /// 是否可卸载
        /// </summary>
        /// <returns></returns>
        public bool CanUnload()
        {
            return UsedAssets.Count == 0 && RefBundles.Count == 0;
        }

        public int CompareTo(BundleRuntimeInfo other)
        {
            return Manifest.CompareTo(other.Manifest);
        }

        public bool Equals(BundleRuntimeInfo other)
        {
            return Manifest.Equals(other.Manifest);
        }

        public override string ToString()
        {
            return Manifest.ToString();
        }
    }
}

