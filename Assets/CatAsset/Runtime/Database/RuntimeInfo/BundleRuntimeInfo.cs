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
        /// 状态
        /// </summary>
        public enum State
        {
            None,
            
            /// <summary>
            /// 位于只读区
            /// </summary>
            InReadOnly,
            
            /// <summary>
            /// 位于读写区
            /// </summary>
            InReadWrite,
            
            /// <summary>
            /// 位于远端
            /// </summary>
            InRemote,
        }
        
        /// <summary>
        /// 资源包清单信息
        /// </summary>
        public BundleManifestInfo Manifest;

        /// <summary>
        /// 资源包实例
        /// </summary>
        public AssetBundle Bundle;

        /// <summary>
        /// 资源包状态
        /// </summary>
        public State BundleState;

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
                    switch (BundleState)
                    {
                        case State.InReadOnly:
                            loadPath = RuntimeUtil.GetReadOnlyPath(Manifest.RelativePath);
                            break;
                        case State.InReadWrite:
                            loadPath = RuntimeUtil.GetReadWritePath(Manifest.RelativePath);
                            break;
                        default:
                            Debug.LogError($"资源包：{this}的loadPath获取错误，资源包状态：{BundleState}");
                            break;
                    }
                }
                return loadPath;
            }
        }

        /// <summary>
        /// 当前使用中的资源集合，这里面的资源的引用计数都大于0
        /// </summary>
        public readonly HashSet<AssetRuntimeInfo> UsingAssets = new HashSet<AssetRuntimeInfo>();

        /// <summary>
        /// 资源包依赖链
        /// </summary>
        public readonly BundleDependencyLink DependencyLink = new BundleDependencyLink();

        /// <summary>
        /// 添加使用中的资源
        /// </summary>
        public void AddUsingAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            UsingAssets.Add(assetRuntimeInfo);
        }
        
        /// <summary>
        /// 移除使用中的资源
        /// </summary>
        public void RemoveUsingAsset(AssetRuntimeInfo assetRuntimeInfo)
        {
            UsingAssets.Remove(assetRuntimeInfo);
            CheckLifeCycle();
        }

        /// <summary>
        /// 检查资源包生命周期
        /// </summary>
        public void CheckLifeCycle()
        {
            if (!Manifest.IsRaw && UsingAssets.Count == 0 && DependencyLink.DownStream.Count == 0)
            {
                //此资源包不是原生资源包 没有资源在使用中 没有下游资源包
                //卸载资源包
                CatAssetManager.AddUnloadBundleTask(this);
            }
        }

        /// <summary>
        /// 添加下游资源包（依赖此资源包的资源包）
        /// </summary>
        public void AddDownStream(BundleRuntimeInfo bundleRuntimeInfo)
        {
            DependencyLink.DownStream.Add(bundleRuntimeInfo);
        }

        /// <summary>
        /// 移除下游资源包（依赖此资源包的资源包）
        /// </summary>
        public void RemoveDownStream(BundleRuntimeInfo bundleRuntimeInfo)
        {
            DependencyLink.DownStream.Remove(bundleRuntimeInfo);
        }

        /// <summary>
        /// 添加上游资源包（此资源包依赖的资源包）
        /// </summary>
        public void AddUpStream(BundleRuntimeInfo bundleRuntimeInfo)
        {
            DependencyLink.UpStream.Add(bundleRuntimeInfo);
        }
        
        /// <summary>
        /// 清空上游资源包（此资源包依赖的资源包）
        /// </summary>
        public void ClearUpStream()
        {
            DependencyLink.UpStream.Clear();
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

