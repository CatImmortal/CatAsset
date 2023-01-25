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

        //资源包相对路径 -> 分析器资源包信息列表索引
        private static Dictionary<string, int> tempPbiDict =
            new Dictionary<string, int>();

        //资源名 -> 分析器资源信息列表索引
        private static Dictionary<string, int> tempPaiDict =
            new Dictionary<string, int>();

        /// <summary>
        /// 分析器资源信息列表索引 -> 分析器资源包信息列表索引
        /// </summary>
        private static Dictionary<int, int> tempPaiIndex2PbiIndexDict = new Dictionary<int, int>();

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
        /// 清空所有资源组信息
        /// </summary>
        internal static void ClearAllGroupInfo()
        {
            groupInfoDict.Clear();
        }

        /// <summary>
        /// 获取调试分析器数据
        /// </summary>
        public static ProfilerInfo GetProfilerInfo()
        {

            ProfilerInfo info = ProfilerInfo.Create();

            BuildProfilerBundleInfo(info);
            BuildProfilerTaskInfo(info);
            BuildProfilerGroupInfo(info);
            BuildProfilerUpdaterInfo(info);

            return info;
        }

        /// <summary>
        /// 构建分析器资源包信息
        /// </summary>
        private static void BuildProfilerBundleInfo(ProfilerInfo info)
        {
            tempPbiDict.Clear();
            tempPaiDict.Clear();
            tempPaiIndex2PbiIndexDict.Clear();

            //先建立分析器信息到索引的映射
            foreach (var pair in bundleRuntimeInfoDict)
            {
                var bri = pair.Value;

                if (!bri.Manifest.IsRaw)
                {
                    if (bri.Bundle == null)
                    {
                        //跳过未加载的 的非原生资源包
                        continue;
                    }
                }
                else
                {
                    var ari = GetAssetRuntimeInfo(bri.Manifest.Assets[0].Name);
                    if (ari.Asset == null)
                    {
                        //跳过未加载的原生资源
                        continue;
                    }
                }

                ProfilerBundleInfo pbi = ProfilerBundleInfo.Create(bri.Manifest.RelativePath, bri.Manifest.Group,
                    bri.Manifest.IsRaw, bri.Manifest.Length,bri.ReferencingAssets.Count, bri.Manifest.Assets.Count);

                int pbiIndex = info.BundleInfoList.Count;
                info.BundleInfoList.Add(pbi);
                tempPbiDict.Add(pbi.RelativePath, pbiIndex);

                foreach (var ami in bri.Manifest.Assets)
                {
                    var ari = GetAssetRuntimeInfo(ami.Name);

                    //跳过未加载的非场景资源 或者 引用计数为0的场景资源
                    if (!bri.Manifest.IsScene)
                    {
                        if (ari.Asset == null)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (ari.RefCount == 0)
                        {
                            continue;
                        }
                    }

                    string type = bri.Manifest.IsScene ? "Scene" : ari.Asset.GetType().Name;

                    ProfilerAssetInfo pai = ProfilerAssetInfo.Create(ari.AssetManifest.Name,type, ari.MemorySize,
                        ari.RefCount);
                    int paiIndex = info.AssetInfoList.Count;
                    tempPaiDict.Add(pai.Name, paiIndex);
                    tempPaiIndex2PbiIndexDict.Add(paiIndex,pbiIndex);
                    info.AssetInfoList.Add(pai);

                }
            }

            //建立对索引的记录
            foreach (var pbiPair in tempPbiDict)
            {
                var pbiIndex = pbiPair.Value;
                var pbi = info.BundleInfoList[pbiIndex];
                var bri = GetBundleRuntimeInfo(pbi.RelativePath);

                //资源包依赖链索引
                foreach (var upBri in bri.DependencyChain.UpStream)
                {
                    var upPbiIndex = tempPbiDict[upBri.Manifest.RelativePath];
                    pbi.UpStreamIndexes.Add(upPbiIndex);
                }
                foreach (var downBri in bri.DependencyChain.DownStream)
                {
                    var downPbiIndex = tempPbiDict[downBri.Manifest.RelativePath];
                    pbi.DownStreamIndexes.Add(downPbiIndex);
                }
            }

            foreach (var paiPair in tempPaiDict)
            {
                var paiIndex = paiPair.Value;
                var pai = info.AssetInfoList[paiIndex];
                var ari = GetAssetRuntimeInfo(pai.Name);

                //资源包中在内存中的资源索引
                var pbiIndex = tempPaiIndex2PbiIndexDict[paiIndex];
                var pbi = info.BundleInfoList[pbiIndex];
                pbi.InMemoryAssetIndexes.Add(paiIndex);

                //资源依赖链索引
                foreach (var upAri in ari.DependencyChain.UpStream)
                {
                    var upPaiIndex = tempPaiDict[upAri.AssetManifest.Name];
                    pai.UpStreamIndexes.Add(upPaiIndex);
                }

                foreach (var downAri in ari.DependencyChain.DownStream)
                {
                    if (!tempPaiDict.ContainsKey(downAri.AssetManifest.Name))
                    {
                        continue;
                    }
                    
                    var downPaiIndex = tempPaiDict[downAri.AssetManifest.Name];
                    pai.DownStreamIndexes.Add(downPaiIndex);
                }
            }
        }

        /// <summary>
        /// 构建分析器任务信息
        /// </summary>
        private static void BuildProfilerTaskInfo(ProfilerInfo info)
        {
            foreach (var pair in TaskRunner.MainTaskDict)
            {
                foreach (var pair2 in pair.Value)
                {
                    var task = pair2.Value;

                    ProfilerTaskInfo pti = ProfilerTaskInfo.Create(task.Name,task.GetType().Name,task.State,task.Progress,task.MergedTaskCount);
                    info.TaskInfoList.Add(pti);
                }
            }
            info.TaskInfoList.Sort();
        }

        /// <summary>
        /// 构建分析器资源组信息
        /// </summary>
        private static void BuildProfilerGroupInfo(ProfilerInfo info)
        {
            foreach (var pair in groupInfoDict)
            {
                var group = pair.Value;

                ProfilerGroupInfo pgi = ProfilerGroupInfo.Create(group.GroupName, group.LocalCount, group.LocalLength,
                    group.RemoteCount, group.RemoteLength
                );

                info.GroupInfoList.Add(pgi);
            }
            info.GroupInfoList.Sort();
        }

        /// <summary>
        /// 构建分析器更新器信息
        /// </summary>
        private static void BuildProfilerUpdaterInfo(ProfilerInfo info)
        {
            foreach (var pair in CatAssetUpdater.GroupUpdaterDict)
            {
                var updater = pair.Value;
                ProfilerUpdaterInfo pui = ProfilerUpdaterInfo.Create(updater.GroupName, updater.UpdatedCount, updater.UpdatedLength, updater.TotalCount,
                    updater.TotalLength,updater.Speed, updater.State);

                info.UpdaterInfoList.Add(pui);
            }
            info.UpdaterInfoList.Sort();
        }
    }
}
