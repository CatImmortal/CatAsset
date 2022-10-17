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
        public int UseCount { get; private set; }

        /// <summary>
        /// 上游资源集合（依赖此资源的资源）
        /// </summary>
        public readonly HashSet<AssetRuntimeInfo> UpStream = new HashSet<AssetRuntimeInfo>();

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void AddUseCount(int count = 1)
        {
            if (IsUnused())
            {
                //引用计数从0变为1
                //添加到资源包的已使用资源集合中
                BundleRuntimeInfo bundleRuntimeInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(BundleManifest.RelativePath);
                bundleRuntimeInfo.StartUseAsset(this);
            }
            
            UseCount += count;
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void SubUseCount(int count = 1)
        {
            if (UseCount == 0)
            {
                Debug.LogError($"尝试减少引用计数为0的资源的引用计数:{this}");
                return;
            }

            UseCount -= count;

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
            return UseCount == 0;
        }

        /// <summary>
        /// 添加上游资源（依赖此资源的资源）
        /// </summary>
        public void AddUpStream(AssetRuntimeInfo assetRuntimeInfo)
        {
            if (Asset == null)
            {
                return;
            }
            UpStream.Add(assetRuntimeInfo);
        }

        /// <summary>
        /// 移除上游资源（依赖此资源的资源）
        /// </summary>
        public void RemoveUpStream(AssetRuntimeInfo assetRuntimeInfo)
        {
            if (Asset == null)
            {
                return;
            }
            UpStream.Remove(assetRuntimeInfo);
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