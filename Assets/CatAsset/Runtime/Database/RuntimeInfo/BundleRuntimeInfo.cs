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
        /// 引用了此资源包的资源包集合
        /// </summary>
        public HashSet<BundleRuntimeInfo> RefBundles{ get; } = new HashSet<BundleRuntimeInfo>();
        
        /// <summary>
        /// 此资源包依赖的资源包集合
        /// </summary>
        public HashSet<BundleRuntimeInfo> DependencyBundles { get; } = new HashSet<BundleRuntimeInfo>();

        /// <summary>
        /// 开始使用资源
        /// </summary>
        public void StartUseAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            UsedAssets.Add(assetRuntimeInfo);
        }
        
        /// <summary>
        /// 停止使用资源
        /// </summary>
        public void EndUseAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            UsedAssets.Remove(assetRuntimeInfo);

            if (CanUnload())
            {
                //此资源包没有资源在使用中了 并且没有其他资源包引用它 卸载资源包
                
                if (!Manifest.IsRaw)
                {
                    CatAssetManager.UnloadBundle(this);
                }
                else
                {
                    //一个原生资源包只对应一个唯一的原生资源
                    CatAssetManager.UnloadRawAsset(this,assetRuntimeInfo);
                }
                
            }
        }

        /// <summary>
        /// 添加引用了此资源包的资源包
        /// </summary>
        public void AddRefBundle(BundleRuntimeInfo bundleRuntimeInfo)
        {
            RefBundles.Add(bundleRuntimeInfo);
        }

        /// <summary>
        /// 移除引用了此资源包的资源包
        /// </summary>
        public void RemoveRefBundle(BundleRuntimeInfo bundleRuntimeInfo)
        {
            RefBundles.Remove(bundleRuntimeInfo);
        }

        /// <summary>
        /// 添加此资源包依赖的资源包
        /// </summary>
        public void AddDependencyBundle(BundleRuntimeInfo bundleRuntimeInfo)
        {
            DependencyBundles.Add(bundleRuntimeInfo);
        }

        /// <summary>
        /// 移除此资源包依赖的资源包
        /// </summary>
        public void RemoveDependencyBundle(BundleRuntimeInfo bundleRuntimeInfo)
        {
            DependencyBundles.Remove(bundleRuntimeInfo);
        }
        
        /// <summary>
        /// 是否可卸载
        /// </summary>
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

