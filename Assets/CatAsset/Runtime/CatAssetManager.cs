using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace CatAsset
{
    /// <summary>
    /// CatAsset管理器
    /// </summary>
    public class CatAssetManager
    {
        /// <summary>
        /// AssetBundle运行时信息字典
        /// </summary>
        private static Dictionary<string, AssetBundleRuntimeInfo> assetBundleInfoDict = new Dictionary<string, AssetBundleRuntimeInfo>();
        
        /// <summary>
        /// Asset运行时信息字典
        /// </summary>
        private static Dictionary<string, AssetRuntimeInfo> assetInfoDict = new Dictionary<string, AssetRuntimeInfo>();

        /// <summary>
        /// Asset到Asset运行时信息的映射
        /// </summary>
        private static Dictionary<Object, AssetRuntimeInfo> AssetToRuntimeInfo = new Dictionary<Object, AssetRuntimeInfo>();

        /// <summary>
        /// 使用资源清单初始化资源数据
        /// </summary>
        public static void CheckManifest(CatAssetManifest manifest)
        {
            foreach (AssetBundleManifestInfo abManifestInfo in manifest.AssetBundles)
            {
                AssetBundleRuntimeInfo abRuntimeInfo = new AssetBundleRuntimeInfo();
                assetBundleInfoDict.Add(abManifestInfo.AssetBundleName, abRuntimeInfo);

                abRuntimeInfo.ManifestInfo = abManifestInfo;

                foreach (AssetManifestInfo assetManifestInfo in abManifestInfo.Assets)
                {
                    AssetRuntimeInfo assetRuntimeInfo = new AssetRuntimeInfo();
                    assetInfoDict.Add(assetManifestInfo.AssetName, assetRuntimeInfo);

                    assetRuntimeInfo.ManifestInfo = assetManifestInfo;
                    assetRuntimeInfo.AssetBundleName = abManifestInfo.AssetBundleName;
                }
            }
        }

        /// <summary>
        /// 加载Asset
        /// </summary>
        public static void LoadAsset<T>(string assetName,Action<T> callback) where T : Object
        {
            if (!assetInfoDict.TryGetValue(assetName,out AssetRuntimeInfo assetInfo))
            {
                throw new Exception("Asset加载失败，该Asset不在资源清单中");
            }

            if (assetInfo.Asset != null)
            {
                //Asset已被加载过了

                assetInfo.UseCount++;
                callback(assetInfo.Asset as T);
                return;
            }

            //Asset未加载 
            AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
            if (abInfo.AssetBundle != null)
            {
                //AssetBundle已被加载 创建加载Asset与其依赖的Asset的任务
                return;
            }

            //AssetBundle未加载 创建加载AssetBundle的任务

        }

        /// <summary>
        /// 卸载Asset
        /// </summary>
        public static void UnloadAsset(Object asset)
        {
            if (!AssetToRuntimeInfo.TryGetValue(asset,out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("要卸载的Asset不是从CatAsset加载的");
            }

            //减少Asset的引用计数
            assetInfo.UseCount--;

            //卸载依赖资源
            foreach (string dependency in assetInfo.ManifestInfo.Dependencies)
            {
                AssetRuntimeInfo dependencyInfo = assetInfoDict[dependency];
                UnloadAsset(dependencyInfo.Asset);
            }

            if (assetInfo.UseCount == 0)
            {
                //Asset不被使用了
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAsset.Remove(assetInfo.ManifestInfo.AssetName);

                if (abInfo.UsedAsset.Count == 0)
                {
                    //AssetBundel没有Assset被使用了 加入待卸载列表
                }
            }
        }
  

    }
}

