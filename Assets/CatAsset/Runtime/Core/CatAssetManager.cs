using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

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
        /// Asset和Asset运行时信息的关联(不包括场景)
        /// </summary>
        private static Dictionary<Object, AssetRuntimeInfo> assetToAssetInfo = new Dictionary<Object, AssetRuntimeInfo>();

        /// <summary>
        /// 任务执行器
        /// </summary>
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
        /// 添加Asset到Asset运行时信息的映射
        /// </summary>
        public static void AddAssetToRuntimeInfo(AssetRuntimeInfo info)
        {
            assetToAssetInfo.Add(info.Asset, info);
        }

        /// <summary>
        /// 移除Asset到Asset运行时信息的映射
        /// </summary>
        public static void RemoveAssetToRuntimeInfo(AssetRuntimeInfo info)
        {
            assetToAssetInfo.Remove(info.Asset);
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

            Debug.Log("资源清单检查完毕，版本号：" + manifest.GameVersion + "." + manifest.ManifestVersion);
        }


        /// <summary>
        /// 加载Asset
        /// </summary>
        public static void LoadAsset(string assetName,Action<object> loadedCallback,int priority = 0)
        {
            if (assetBundleInfoDict.Count == 0)
            {
                Debug.LogError("Asset加载失败,未调用CheckManifest进行资源清单检查");
                return;
            }

            if (!assetInfoDict.TryGetValue(assetName,out AssetRuntimeInfo assetInfo))
            {
                throw new Exception("Asset加载失败，不在资源清单中：" + assetName);
            }

            //加载依赖Asset 已加载的就增加它们的引用计数 未加载的就创建加载任务
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependency = assetInfo.ManifestInfo.Dependencies[i];
                LoadAsset(dependency, null, priority + 1);
            }


            if (assetInfo.UseCount == 0)
            {
                //标记进 所属的AssetBundle的使用中Asset集合 中
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAsset.Add(assetInfo.ManifestInfo.AssetName);
            }
            //增加引用计数
            assetInfo.UseCount++;

            if (assetInfo.Asset != null) 
            {
                //已加 直接调用回调方法
                loadedCallback?.Invoke(assetInfo.Asset);
                return;
            }

            //未加载 创建加载Asset的任务
            LoadAssetTask task = new LoadAssetTask(taskExcutor, assetName,priority, loadedCallback, assetInfo);
            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 卸载Asset
        /// </summary>
        public static void UnloadAsset(Object asset)
        {
            if (!assetToAssetInfo.TryGetValue(asset,out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("要卸载的Asset不是从CatAsset加载的：" + asset.name);
                return;
            }

            //卸载依赖资源
            foreach (string dependency in assetInfo.ManifestInfo.Dependencies)
            {
                AssetRuntimeInfo dependencyInfo = assetInfoDict[dependency];
                UnloadAsset(dependencyInfo.Asset);
            }

            //减少Asset的引用计数
            assetInfo.UseCount--;
            if (assetInfo.UseCount == 0)
            {
                //Asset已经没人使用了
                //从 所属的AssetBundle的使用中Asset集合 中移除
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAsset.Remove(assetInfo.ManifestInfo.AssetName);

                if (abInfo.UsedAsset.Count == 0)
                {
                    //AssetBundle已经没人使用了 创建卸载任务 开始卸载倒计时
                    UnloadAssetBundleTask task = new UnloadAssetBundleTask(taskExcutor, abInfo.ManifestInfo.AssetBundleName, 0, null, abInfo);
                    taskExcutor.AddTask(task);
                    Debug.Log("创建了卸载AB的任务：" + task.Name);
                }
            }
        }
 
        /// <summary>
        /// 加载场景
        /// </summary>
        public static void LoadScene(string sceneName, Action<object> loadedCallback, int priority = 0)
        {
            if (assetBundleInfoDict.Count == 0)
            {
                Debug.LogError("场景加载失败,未调用CheckManifest进行资源清单检查");
                return;
            }

            if (!assetInfoDict.TryGetValue(sceneName, out AssetRuntimeInfo assetInfo))
            {
                throw new Exception("场景加载失败，该场景不在资源清单中：" + sceneName);
            }

            //加载依赖Asset 已加载的就增加它们的引用计数 未加载的就创建加载任务
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependency = assetInfo.ManifestInfo.Dependencies[i];
                LoadAsset(dependency, null, priority + 1);
            }


            if (assetInfo.UseCount == 0)
            {
                //标记进 所属的AssetBundle的使用中Asset集合 中
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAsset.Add(assetInfo.ManifestInfo.AssetName);
            }
            //增加引用计数
            assetInfo.UseCount++;

            //场景资源实例不能被复用 每次加载都得创建加载场景的任务
            LoadSceneTask task = new LoadSceneTask(taskExcutor, sceneName, priority, loadedCallback, assetInfo);
            taskExcutor.AddTask(task);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public static void UnloadScene(string sceneName)
        {
            if (!assetInfoDict.TryGetValue(sceneName,out AssetRuntimeInfo assetInfo))
            {
                Debug.LogError("要卸载的Scene不在资源清单中：" + sceneName);
                return;
            }

            if (assetInfo.UseCount == 0)
            {
                Debug.LogError("要卸载的场景未加载过：" + sceneName);
                return;
            }

            //卸载依赖资源
            foreach (string dependency in assetInfo.ManifestInfo.Dependencies)
            {
                AssetRuntimeInfo dependencyInfo = assetInfoDict[dependency];
                UnloadAsset(dependencyInfo.Asset);
            }

            //卸载场景
            SceneManager.UnloadSceneAsync(sceneName);

            //减少场景的引用计数
            assetInfo.UseCount--;
            if (assetInfo.UseCount == 0)
            {
                //场景已经没人使用了
                //从 所属的AssetBundle的使用中Asset集合 中移除
                AssetBundleRuntimeInfo abInfo = assetBundleInfoDict[assetInfo.AssetBundleName];
                abInfo.UsedAsset.Remove(assetInfo.ManifestInfo.AssetName);

                if (abInfo.UsedAsset.Count == 0)
                {
                    //AssetBundel已经没人使用了 创建卸载任务 开始卸载倒计时
                    UnloadAssetBundleTask task = new UnloadAssetBundleTask(taskExcutor, abInfo.ManifestInfo.AssetBundleName, 0, null, abInfo);
                    taskExcutor.AddTask(task);
                    Debug.Log("创建了卸载AB的任务：" + task.Name);
                }
            }

        }

        /// <summary>
        /// 批量加载Asset
        /// </summary>
        public static void LoadAssets(List<string> assetNames, Action<object> loadedCallback, int priority = 0)
        {
            if (assetBundleInfoDict.Count == 0)
            {
                Debug.LogError("Asset加载失败,未调用CheckManifest进行资源清单检查");
                return;
            }

            //创建批量加载Asset的任务
            LoadAssetsTask task = new LoadAssetsTask(taskExcutor, nameof(LoadAssetsTask), priority, loadedCallback, assetNames);
            taskExcutor.AddTask(task);
        }
    }
}

