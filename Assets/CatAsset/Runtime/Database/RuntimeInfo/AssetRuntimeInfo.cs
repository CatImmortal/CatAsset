using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源运行时信息
    /// </summary>
    public class AssetRuntimeInfo : IComparable<AssetRuntimeInfo>, IEquatable<AssetRuntimeInfo>
    {
        /// <summary>
        /// 所在资源包清单信息
        /// </summary>
        public BundleManifestInfo BundleManifest;

        /// <summary>
        /// 资源清单信息
        /// </summary>
        public AssetManifestInfo AssetManifest;

        /// <summary>
        /// 已加载的资源实例
        /// </summary>
        public object Asset;
        
        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// 引用了此资源的资源集合
        /// </summary>
        public HashSet<AssetRuntimeInfo> RefAssets { get; } = new HashSet<AssetRuntimeInfo>();

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void AddRefCount(int count = 1)
        {
            if (IsUnused())
            {
                //引用计数从0变为1
                //添加到资源包的已使用资源集合中
                BundleRuntimeInfo bundleRuntimeInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(BundleManifest.RelativePath);
                bundleRuntimeInfo.UseAsset(this);
            }
            
            RefCount += count;
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void SubRefCount(int count = 1)
        {
            if (RefCount == 0)
            {
                Debug.LogError($"尝试减少引用计数为0的资源的引用计数:{this}");
                return;
            }

            RefCount -= count;

            if (IsUnused())
            {
                //引用计数从1变为0
                //从资源包的已使用资源集合中删除
                BundleRuntimeInfo bundleRuntimeInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(BundleManifest.RelativePath);
                bundleRuntimeInfo.EndUseAsset(this);
            }
        }
        
        /// <summary>
        /// 是否未被使用
        /// </summary>
        public bool IsUnused()
        {
            return RefCount == 0;
        }

        /// <summary>
        /// 添加引用了此资源的资源
        /// </summary>
        /// <param name="assetRuntimeInfo"></param>
        public void AddRefAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            if (Asset == null)
            {
                return;
            }
            RefAssets.Add(assetRuntimeInfo);
        }

        /// <summary>
        /// 移除引用了此资源的资源
        /// </summary>
        public void RemoveRefAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            if (Asset == null)
            {
                return;
            }
            RefAssets.Remove(assetRuntimeInfo);
        }
        
        public int CompareTo(AssetRuntimeInfo other)
        {
            return AssetManifest.CompareTo(other.AssetManifest);
        }

        public bool Equals(AssetRuntimeInfo other)
        {
            return BundleManifest.Equals(other.BundleManifest) && AssetManifest.Equals(other.AssetManifest);
        }

        public override string ToString()
        {
            return AssetManifest.ToString();
        }
    }
}