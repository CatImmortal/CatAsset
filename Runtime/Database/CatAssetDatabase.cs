using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
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
        /// 场景实例handler->绑定的资源句柄
        /// </summary>
        private static Dictionary<int, List<IBindableHandler>> sceneBindHandlers =
            new Dictionary<int, List<IBindableHandler>>();

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
                InitRuntimeInfo(info,BundleRuntimeInfo.State.InReadOnly);
            }
        }

        /// <summary>
        /// 根据资源包清单信息初始化运行时信息
        /// </summary>
        internal static void InitRuntimeInfo(BundleManifestInfo bundleManifestInfo, BundleRuntimeInfo.State state)
        {
            BundleRuntimeInfo bundleRuntimeInfo = new BundleRuntimeInfo();
            bundleRuntimeInfoDict.Add(bundleManifestInfo.RelativePath, bundleRuntimeInfo);
            bundleRuntimeInfo.Manifest = bundleManifestInfo;
            bundleRuntimeInfo.BundleState = state;

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
                bundleRuntimeInfoDict.Add(bundleRuntimeInfo.Manifest.RelativePath,bundleRuntimeInfo);
            }
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
        /// 获取调试分析器数据
        /// </summary>
        public static ProfilerInfo GetProfilerInfo(ProfilerInfoType type)
        {

            ProfilerInfo info = new ProfilerInfo { Type = type };

            switch (type)
            {
                case ProfilerInfoType.Bundle:
                    BuildProfilerBundleInfo(info);
                    break;

                case ProfilerInfoType.Task:
                    break;

                case ProfilerInfoType.Group:
                    break;

                case ProfilerInfoType.Updater:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return info;
        }

        /// <summary>
        /// 构建分析器资源包信息
        /// </summary>
        private static void BuildProfilerBundleInfo(ProfilerInfo info)
        {
            info.AssetInfoList = new List<ProfilerAssetInfo>();
            info.BundleInfoList = new List<ProfilerBundleInfo>();

            //资源包相对路径 -> 列表索引
            Dictionary<string, int> pbiDict =
                new Dictionary<string, int>();

            //资源名 -> 列表索引
            Dictionary<string, int> paiDict =
                new Dictionary<string, int>();


            //先建立分析器信息到索引的映射
            foreach (var pair in bundleRuntimeInfoDict)
            {
                var bri = pair.Value;
                if (!bri.Manifest.IsRaw && bri.Bundle == null && bri.DependencyChain.UpStream.Count == 0 &&
                    bri.DependencyChain.DownStream.Count == 0)
                {
                    //跳过未加载 也没有上下游的资源包
                    continue;
                }

                var pbi = new ProfilerBundleInfo
                {
                    Directory = bri.Manifest.Directory,
                    BundleName = bri.Manifest.BundleName,
                    RelativePath = bri.Manifest.RelativePath,
                    Group = bri.Manifest.Group,
                    Length = bri.Manifest.Length,
                    AssetCount = bri.Manifest.Assets.Count,
                };

                int index = info.BundleInfoList.Count;
                info.BundleInfoList.Add(pbi);
                pbiDict.Add(bri.Manifest.RelativePath, index);

                foreach (var ari in bri.ReferencingAssets)
                {
                    var pai = new ProfilerAssetInfo
                    {
                        Name = ari.AssetManifest.Name, Length = ari.AssetManifest.Length, RefCount = ari.RefCount,
                    };
                    index = info.AssetInfoList.Count;
                    paiDict.Add(pai.Name, index);
                    info.AssetInfoList.Add(pai);
                }
            }

            //建立对索引的记录
            foreach (var pair in bundleRuntimeInfoDict)
            {
                var bri = pair.Value;

                if (!pbiDict.TryGetValue(bri.Manifest.RelativePath, out var pbiIndex))
                {
                    //跳过没有对应分析器信息的资源包
                    continue;
                }

                var pbi = info.BundleInfoList[pbiIndex];

                //资源包依赖链
                foreach (var upBri in bri.DependencyChain.UpStream)
                {
                    var upPbiIndex = pbiDict[upBri.Manifest.RelativePath];
                    pbi.UpStreamIndexes.Add(upPbiIndex);
                }

                foreach (var downBri in bri.DependencyChain.DownStream)
                {
                    var downPbiIndex = pbiDict[downBri.Manifest.RelativePath];
                    pbi.DownStreamIndexes.Add(downPbiIndex);
                }

                foreach (var ari in bri.ReferencingAssets)
                {
                    //资源包中被引用中的资源
                    var paiIndex = paiDict[ari.AssetManifest.Name];
                    pbi.ReferencingAssetIndexes.Add(paiIndex);

                    //资源依赖链
                    var pai = info.AssetInfoList[paiIndex];
                    foreach (var upAri in ari.DependencyChain.UpStream)
                    {
                        var upPaiIndex = paiDict[upAri.AssetManifest.Name];
                        pai.UpStreamIndexes.Add(upPaiIndex);
                    }

                    foreach (var downAri in ari.DependencyChain.DownStream)
                    {
                        var downPaiIndex = paiDict[downAri.AssetManifest.Name];
                        pai.DownStreamIndexes.Add(downPaiIndex);
                    }
                }

            }
        }
    }
}
