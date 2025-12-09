using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源数据库
    /// </summary>
    public static partial class CatAssetDatabase
    {
        /// <summary>
        /// 资源包标识名 -> 资源包运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        private static Dictionary<string, BundleRuntimeInfo> bundleRuntimeInfoDict =
            new Dictionary<string, BundleRuntimeInfo>();

        /// <summary>
        /// 资源名 -> 资源运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        private static Dictionary<string, AssetRuntimeInfo> assetRuntimeInfoDict =
            new Dictionary<string, AssetRuntimeInfo>();

        /// <summary>
        /// 资源实例 -> 资源运行时信息
        /// </summary>
        private static Dictionary<object, AssetRuntimeInfo> assetInstanceDict =
            new Dictionary<object, AssetRuntimeInfo>();

        /// <summary>
        /// 场景实例handler -> 资源运行时信息
        /// </summary>
        private static Dictionary<int, AssetRuntimeInfo> sceneInstanceDict = new Dictionary<int, AssetRuntimeInfo>();

        /// <summary>
        /// 场景实例handler -> 绑定的资源句柄
        /// </summary>
        private static Dictionary<int, List<IBindableHandler>> sceneBindHandlers =
            new Dictionary<int, List<IBindableHandler>>();

      
        
        
        /// <summary>
        /// 使用资源清单进行运行时信息的初始化
        /// </summary>
        internal static void InitRuntimeInfoByManifest(CatAssetManifest manifest)
        {
            lazyBundleInfoDict.Clear();
            lazyAssetInfoDict.Clear();
            
            bundleRuntimeInfoDict.Clear();
            assetRuntimeInfoDict.Clear();


            foreach (BundleManifestInfo info in manifest.Bundles)
            {
                InitRuntimeInfo(info,BundleRuntimeInfo.State.InReadOnly);
            }
        }

        /// <summary>
        /// 根据资源包清单信息初始化运行时信息
        /// </summary>
        internal static void InitRuntimeInfo(BundleManifestInfo bundleManifestInfo, BundleRuntimeInfo.State state)
        {
            LazyBundleInfo lazyBundleInfo = new LazyBundleInfo
            {
                Manifest = bundleManifestInfo,
                State = state
            };
            lazyBundleInfoDict[bundleManifestInfo.BundleIdentifyName] = lazyBundleInfo;

            foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
            {
                LazyAssetInfo lazyAssetInfo = new LazyAssetInfo
                {
                    BundleManifestInfo = bundleManifestInfo,
                    AssetManifestInfo =  assetManifestInfo,
                };
                lazyAssetInfoDict[assetManifestInfo.Name] = lazyAssetInfo;
            }
        }

        /// <summary>
        /// 获取资源包运行时信息
        /// </summary>
        internal static BundleRuntimeInfo GetBundleRuntimeInfo(string bundleIdentifyName)
        {
            BundleRuntimeInfo info;
            if (lazyBundleInfoDict.TryGetValue(bundleIdentifyName,out var lazy))
            {
                lazyBundleInfoDict.Remove(bundleIdentifyName);
                info = new BundleRuntimeInfo();
                info.Manifest = lazy.Manifest;
                info.BundleState = lazy.State;
                bundleRuntimeInfoDict[bundleIdentifyName] = info;
                return info;
            }
            
            bundleRuntimeInfoDict.TryGetValue(bundleIdentifyName, out info);
            return info;
        }

        /// <summary>
        /// 获取所有资源包运行时信息
        /// </summary>
        internal static Dictionary<string, BundleRuntimeInfo> GetAllBundleRuntimeInfo()
        {
            return bundleRuntimeInfoDict;
        }

        /// <summary>
        /// 获取资源运行时信息
        /// </summary>
        internal static AssetRuntimeInfo GetAssetRuntimeInfo(string assetName)
        {
            AssetRuntimeInfo info;
            if (lazyAssetInfoDict.TryGetValue(assetName ,out var lazy))
            {
                lazyAssetInfoDict.Remove(assetName);
                info = new AssetRuntimeInfo();
                info.BundleManifest = lazy.BundleManifestInfo;
                info.AssetManifest = lazy.AssetManifestInfo;
                assetRuntimeInfoDict[assetName] = info;
                return info;
            }
            
            assetRuntimeInfoDict.TryGetValue(assetName, out info);
            return info;
        }


        /// <summary>
        /// 获取资源运行时信息
        /// </summary>
        internal static AssetRuntimeInfo GetAssetRuntimeInfo(object asset)
        {
            assetInstanceDict.TryGetValue(asset, out AssetRuntimeInfo info);
            return info;
        }

        /// <summary>
        /// 设置资源实例与资源运行时信息的关联
        /// </summary>
        internal static void SetAssetInstance(object asset, AssetRuntimeInfo assetRuntimeInfo)
        {
            assetInstanceDict.Add(asset, assetRuntimeInfo);
        }

        /// <summary>
        /// 删除资源实例与资源运行时信息的关联
        /// </summary>
        internal static void RemoveAssetInstance(object asset)
        {
            assetInstanceDict.Remove(asset);
        }


        /// <summary>
        /// 获取场景资源运行时信息
        /// </summary>
        internal static AssetRuntimeInfo GetAssetRuntimeInfo(Scene scene)
        {
            sceneInstanceDict.TryGetValue(scene.handle, out AssetRuntimeInfo info);
            return info;
        }

        /// <summary>
        /// 设置场景实例与资源运行时信息的关联
        /// </summary>
        internal static void SetSceneInstance(Scene scene, AssetRuntimeInfo assetRuntimeInfo)
        {
            sceneInstanceDict.Add(scene.handle, assetRuntimeInfo);
        }

        /// <summary>
        /// 删除场景实例与资源运行时信息的关联
        /// </summary>
        internal static void RemoveSceneInstance(Scene scene)
        {
            sceneInstanceDict.Remove(scene.handle);
        }

        /// <summary>
        /// 尝试创建外置原生资源的运行时信息
        /// </summary>
        internal static void TryCreateExternalRawAssetRuntimeInfo(string assetName)
        {
            if (!assetRuntimeInfoDict.TryGetValue(assetName,out AssetRuntimeInfo assetRuntimeInfo))
            {
                int index = assetName.LastIndexOf('/');
                string dir = null;
                string name;
                if (index == 0)
                {
                    // 虽然不应该出现这种情况，但也先处理一下
                    dir = "/";
                    name = assetName.Substring(1);
                }
                else if (index > 0)
                {
                    //处理多级路径
                    dir = assetName.Substring(0, index);
                    name = assetName.Substring(index + 1);
                }
                else
                {
                    name = assetName;
                }


                //创建外置原生资源的资源运行时信息
                assetRuntimeInfo = new AssetRuntimeInfo();
                assetRuntimeInfo.AssetManifest = new AssetManifestInfo
                {
                    Name = assetName,
                };
                assetRuntimeInfo.BundleManifest = new BundleManifestInfo
                {
                    Directory = dir,
                    BundleName = name,
                    Group = string.Empty,
                    IsRaw = true,
                    IsScene = false,
                    Assets = new List<AssetManifestInfo>(){assetRuntimeInfo.AssetManifest},
                };
                assetRuntimeInfoDict.Add(assetName,assetRuntimeInfo);

                //创建外置原生资源的资源包运行时信息（是虚拟的）
                BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo
                {
                    Manifest = assetRuntimeInfo.BundleManifest,
                    BundleState = BundleRuntimeInfo.State.InReadWrite,
                };
                bundleRuntimeInfoDict.Add(bundleRuntimeInfo.Manifest.BundleIdentifyName,bundleRuntimeInfo);
            }
        }

        /// <summary>
        /// 获取场景绑定的资源句柄列表
        /// </summary>
        internal static List<IBindableHandler> GetSceneBindAssets(Scene scene)
        {
            sceneBindHandlers.TryGetValue(scene.handle, out var handlers);
            return handlers;
        }

        /// <summary>
        /// 添加场景绑定的资源句柄
        /// </summary>
        internal static void AddSceneBindHandler(Scene scene, IBindableHandler handler)
        {
            if (handler.State == HandlerState.InValid)
            {
                //不可绑定无效句柄
                return;
            }

            if (!sceneBindHandlers.TryGetValue(scene.handle,out var handlers))
            {
                handlers = new List<IBindableHandler>();
                sceneBindHandlers.Add(scene.handle,handlers);
            }
            handlers.Add(handler);
        }

        

      
    }
}
