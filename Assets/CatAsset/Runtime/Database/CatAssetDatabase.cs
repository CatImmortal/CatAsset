using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源数据库
    /// </summary>
    public static class CatAssetDatabase
    {
        /// <summary>
        /// 资源包相对路径->资源包运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        private static Dictionary<string, BundleRuntimeInfo> bundleRuntimeInfoDict =
            new Dictionary<string, BundleRuntimeInfo>();

        /// <summary>
        /// 资源名->资源运行时信息（只有在这个字典里的才是在本地可加载的）
        /// </summary>
        private static Dictionary<string, AssetRuntimeInfo> assetRuntimeInfoDict =
            new Dictionary<string, AssetRuntimeInfo>();

        /// <summary>
        /// 资源实例->资源运行时信息
        /// </summary>
        private static Dictionary<object, AssetRuntimeInfo> assetInstanceDict =
            new Dictionary<object, AssetRuntimeInfo>();

        /// <summary>
        /// 场景实例handler->资源运行时信息
        /// </summary>
        private static Dictionary<int, AssetRuntimeInfo> sceneInstanceDict = new Dictionary<int, AssetRuntimeInfo>();

        /// <summary>
        /// 场景实例handler->绑定的资源
        /// </summary>
        private static Dictionary<int, List<AssetRuntimeInfo>> sceneBindAssets =
            new Dictionary<int, List<AssetRuntimeInfo>>();


        /// <summary>
        /// 资源组名->资源组信息
        /// </summary>
        private static Dictionary<string, GroupInfo> groupInfoDict = new Dictionary<string, GroupInfo>();

        /// <summary>
        /// 使用安装包资源清单进行初始化
        /// </summary>
        internal static void InitPackageManifest(CatAssetManifest manifest)
        {
            bundleRuntimeInfoDict.Clear();
            assetRuntimeInfoDict.Clear();
            
            foreach (BundleManifestInfo info in manifest.Bundles)
            {
                InitRuntimeInfo(info, false);
            }
        }
        
        /// <summary>
        /// 根据资源包清单信息初始化运行时信息
        /// </summary>
        internal static void InitRuntimeInfo(BundleManifestInfo bundleManifestInfo, bool inReadWrite)
        {
            BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo();
            bundleRuntimeInfoDict.Add(bundleManifestInfo.RelativePath, bundleRuntimeInfo);
            bundleRuntimeInfo.Manifest = bundleManifestInfo;
            bundleRuntimeInfo.InReadWrite = inReadWrite;

            foreach (AssetManifestInfo assetManifestInfo in bundleManifestInfo.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = new AssetRuntimeInfo();
                assetRuntimeInfoDict.Add(assetManifestInfo.Name, assetRuntimeInfo);
                assetRuntimeInfo.BundleManifest = bundleManifestInfo;
                assetRuntimeInfo.AssetManifest = assetManifestInfo;
            }
        }

        /// <summary>
        /// 获取资源包运行时信息
        /// </summary>
        internal static BundleRuntimeInfo GetBundleRuntimeInfo(string bundleRelativePath)
        {
            bundleRuntimeInfoDict.TryGetValue(bundleRelativePath, out BundleRuntimeInfo info);
            return info;
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
        /// 尝试创建外置原生资源的运行时信息
        /// </summary>
        internal static AssetRuntimeInfo TryCreateExternalRawAssetRuntimeInfo(string assetName)
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
                    InReadWrite = true,
                };
                bundleRuntimeInfoDict.Add(bundleRuntimeInfo.Manifest.RelativePath,bundleRuntimeInfo);
            }

            return assetRuntimeInfo;
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
        /// 获取场景绑定的资源列表
        /// </summary>
        internal static List<AssetRuntimeInfo> GetSceneBindAssets(Scene scene)
        {
            sceneBindAssets.TryGetValue(scene.handle, out List<AssetRuntimeInfo> assets);
            return assets;
        }

        /// <summary>
        /// 添加场景绑定的资源
        /// </summary>
        internal static void AddSceneBindAsset(Scene scene, object asset)
        {
            if (!sceneBindAssets.TryGetValue(scene.handle,out List<AssetRuntimeInfo> assets))
            {
                assets = new List<AssetRuntimeInfo>();
                sceneBindAssets.Add(scene.handle,assets);
            }

            AssetRuntimeInfo info = GetAssetRuntimeInfo(asset);
            assets.Add(info);
          
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
    }
}