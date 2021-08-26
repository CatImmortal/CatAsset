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

        private static TaskExcutor taskExcutor = new TaskExcutor();



        /// <summary>
        /// 设置单帧最大加载数量
        /// </summary>
        public static void SetMaxLoadCount(int maxCount)
        {
            taskExcutor.MaxExcuteCount = maxCount;
        }

        /// <summary>
        /// 获取AssetBundle运行时信息
        /// </summary>
        public static AssetBundleRuntimeInfo GetAssetBundleInfo(string assetBundleName)
        {
            return assetBundleInfoDict[assetBundleName];
        }

        /// <summary>
        /// 获取Asset运行时信息
        /// </summary>
        public static AssetRuntimeInfo GetAssetInfo(string assetName)
        {
            return assetInfoDict[assetName];
        }

        /// <summary>
        /// 轮询CatAsset管理器
        /// </summary>
        public static void Update()
        {
            taskExcutor.Update();
        }

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
                abRuntimeInfo.LoadPath = Application.streamingAssetsPath + "/" + abManifestInfo.AssetBundleName;

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
        public static void LoadAsset(string assetName,Action<object> loadedCallback,int priority = 0)
        {
            if (assetBundleInfoDict.Count == 0)
            {
                Debug.LogError("Asset加载失败,未检查资源清单");
                return;
            }

            if (!assetInfoDict.TryGetValue(assetName,out AssetRuntimeInfo assetInfo))
            {
                throw new Exception("Asset加载失败，该Asset不在资源清单中：" + assetName);
            }

            if (assetInfo.Asset != null)
            {
                //Asset已被加载过了
                assetInfo.UseCount++;
                loadedCallback?.Invoke(assetInfo.Asset);
                return;
            }

            //创建加载Asset的任务
            LoadAssetTask task = new LoadAssetTask(taskExcutor, assetName,priority, loadedCallback, assetInfo);
            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 卸载Asset
        /// </summary>
        public static void UnloadAsset(Object asset)
        {
            if (!AssetToRuntimeInfo.TryGetValue(asset,out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("要卸载的Asset不是从CatAsset加载的");
                return;
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
                //Asset不再被使用了 从AssetBundleRumtimeInfo.UsedAsset中移除
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

