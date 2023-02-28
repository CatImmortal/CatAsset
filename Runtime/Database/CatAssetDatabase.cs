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
        /// 资源组名 -> 资源组信息
        /// </summary>
        private static Dictionary<string, GroupInfo> groupInfoDict = new Dictionary<string, GroupInfo>();

        /// <summary>
        /// 使用资源清单进行运行时信息的初始化
        /// </summary>
        internal static void InitRuntimeInfoByManifest(CatAssetManifest manifest)
        {
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
            BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo();
            bundleRuntimeInfoDict[bundleManifestInfo.BundleIdentifyName] = bundleRuntimeInfo;  //使用覆盖的形式，以实现Mod资源覆盖功能
            bundleRuntimeInfo.Manifest = bundleManifestInfo;
            bundleRuntimeInfo.BundleState = state;

            foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = new AssetRuntimeInfo();
                assetRuntimeInfoDict[assetManifestInfo.Name] = assetRuntimeInfo;  //使用覆盖的形式，以实现Mod资源覆盖功能
                assetRuntimeInfo.BundleManifest = bundleManifestInfo;
                assetRuntimeInfo.AssetManifest = assetManifestInfo;
            }
        }

        /// <summary>
        /// 获取资源包运行时信息
        /// </summary>
        internal static BundleRuntimeInfo GetBundleRuntimeInfo(string bundleIdentifyName)
        {
            bundleRuntimeInfoDict.TryGetValue(bundleIdentifyName, out BundleRuntimeInfo info);
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
            assetRuntimeInfoDict.TryGetValue(assetName, out var info);
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
                if (index >= 0)
                {
                    //处理多级路径
                    dir = assetName.Substring(0, index - 1);
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

        /// <summary>
        /// 获取资源组信息，若不存在则添加
        /// </summary>
        internal static GroupInfo GetOrAddGroupInfo(string group)
        {
            if (!groupInfoDict.TryGetValue(group, out GroupInfo groupInfo))
            {
                groupInfo = new GroupInfo();
                groupInfo.GroupName = group;
                groupInfoDict.Add(group, groupInfo);
            }

            return groupInfo;
        }

        /// <summary>
        /// 获取资源组信息
        /// </summary>
        internal static GroupInfo GetGroupInfo(string group)
        {
            groupInfoDict.TryGetValue(group, out GroupInfo groupInfo);
            return groupInfo;
        }

        /// <summary>
        /// 获取所有资源组信息
        /// </summary>
        internal static List<GroupInfo> GetAllGroupInfo()
        {
            List<GroupInfo> groupInfos = groupInfoDict.Values.ToList();
            return groupInfos;
        }

        /// <summary>
        /// 清空所有资源组信息
        /// </summary>
        internal static void ClearAllGroupInfo()
        {
            groupInfoDict.Clear();
        }

      
    }
}
