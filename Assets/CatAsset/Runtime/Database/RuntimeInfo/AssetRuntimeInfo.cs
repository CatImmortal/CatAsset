using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源运行时信息
    /// </summary>
    public class AssetRuntimeInfo : IComparable<AssetRuntimeInfo>, IEquatable<AssetRuntimeInfo>,
        IDependencyChainOwner<AssetRuntimeInfo>
    {
        /// <summary>
        /// 所属资源包的清单信息
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
        public int RefCount { get; internal set; }

        /// <summary>
        /// 资源依赖链
        /// </summary>
        public DependencyChain<AssetRuntimeInfo> DependencyChain { get; } = new DependencyChain<AssetRuntimeInfo>();

        /// <summary>
        /// 下游资源记录（运行过程中至少依赖加载过此资源一次的资源）
        /// </summary>
        public HashSet<AssetRuntimeInfo> DownStreamRecord { get; } = new HashSet<AssetRuntimeInfo>();

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void AddRefCount()
        {
            RefCount += 1;

            if (RefCount == 1)
            {
                //被重新使用了
                BundleRuntimeInfo bundleRuntimeInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(BundleManifest.RelativePath);
                bundleRuntimeInfo.AddReferencingAsset(this);
            }
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void SubRefCount()
        {
            if (RefCount == 0)
            {
                Debug.LogError($"尝试减少引用计数为0的资源的引用计数:{this}");
                return;
            }

            RefCount -= 1;

            if (IsUnused())
            {
                //未被使用了

                //从资源包的引用中资源集合删除
                BundleRuntimeInfo bundleRuntimeInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(BundleManifest.RelativePath);
                bundleRuntimeInfo.RemoveReferencingAsset(this);

                //尝试从内存卸载
                CatAssetManager.TryUnloadAssetFromMemory(this);
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
        /// 是否有还在内存中的下游资源
        /// </summary>
        private bool IsDownStreamInMemory()
        {
            foreach (AssetRuntimeInfo info in DownStreamRecord)
            {
                if (info.Asset != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否可被卸载
        /// </summary>
        public bool CanUnload()
        {
            if (Asset == null)
            {
                return false;
            }

            if (!IsUnused())
            {
                //不可卸载使用中的资源
                return false;
            }

            if (Asset is GameObject)
            {
                //不可卸载Prefab资源
                return false;
            }

            if (IsDownStreamInMemory())
            {
                //不可卸载下游资源还在内存中的 防止下游资源错误丢失依赖
                return false;
            }

            return true;
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
