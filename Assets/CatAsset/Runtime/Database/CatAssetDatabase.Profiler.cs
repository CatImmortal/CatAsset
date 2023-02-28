using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    public static partial class CatAssetDatabase
    {
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
        /// 获取调试分析器数据
        /// </summary>
        public static ProfilerInfo GetProfilerInfo()
        {
            ProfilerInfo info = ProfilerInfo.Create();

            BuildProfilerBundleInfo(info);
            BuildProfilerTaskInfo(info);
            BuildProfilerGroupInfo(info);
            BuildProfilerUpdaterInfo(info);
            BuildProfilerPoolInfo(info);
            
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

                ProfilerBundleInfo pbi = ProfilerBundleInfo.Create(bri.Manifest.BundleIdentifyName, bri.BundleState,
                    bri.Manifest.Group,
                    bri.Manifest.IsRaw, bri.Manifest.Length, bri.ReferencingAssets.Count, bri.Manifest.Assets.Count);

                int pbiIndex = info.BundleInfoList.Count;
                info.BundleInfoList.Add(pbi);
                tempPbiDict.Add(pbi.BundleIdentifyName, pbiIndex);

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
                var bri = GetBundleRuntimeInfo(pbi.BundleIdentifyName);

                //资源包依赖链索引
                foreach (var upBri in bri.DependencyChain.UpStream)
                {
                    var upPbiIndex = tempPbiDict[upBri.Manifest.BundleIdentifyName];
                    pbi.UpStreamIndexes.Add(upPbiIndex);
                }
                foreach (var downBri in bri.DependencyChain.DownStream)
                {
                    var downPbiIndex = tempPbiDict[downBri.Manifest.BundleIdentifyName];
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
                    foreach (var pair3 in pair2.Value)
                    {
                        var task = pair3.Value;

                        ProfilerTaskInfo pti = ProfilerTaskInfo.Create(task.Name,task.GetType().Name,task.State,task.Progress,task.MergedTaskCount);
                        info.TaskInfoList.Add(pti);
                    }
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

                List<ProfilerGroupInfo.BundleInfo> localBundles = new List<ProfilerGroupInfo.BundleInfo>();
                foreach (string bundle in group.LocalBundles)
                {
                    var bri =  GetBundleRuntimeInfo(bundle);
                    ProfilerGroupInfo.BundleInfo pgiBundleInfo = ProfilerGroupInfo.BundleInfo.Create(bri.Manifest.BundleIdentifyName,bri.BundleState,bri.Manifest.Length);
                    localBundles.Add(pgiBundleInfo);
                }
                localBundles.Sort();
                
                List<ProfilerGroupInfo.BundleInfo> remoteBundles = new List<ProfilerGroupInfo.BundleInfo>();
                foreach (string bundle in group.RemoteBundles)
                {
                    var bri =  GetBundleRuntimeInfo(bundle);
                    ProfilerGroupInfo.BundleInfo pgiBundleInfo = ProfilerGroupInfo.BundleInfo.Create(bri.Manifest.BundleIdentifyName,bri.BundleState,bri.Manifest.Length);
                    remoteBundles.Add(pgiBundleInfo);
                }
                remoteBundles.Sort();

                ProfilerGroupInfo pgi = ProfilerGroupInfo.Create(group.GroupName, localBundles,
                    group.LocalLength, remoteBundles, group.RemoteLength
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
                GroupUpdater updater = pair.Value;

                List<ProfilerUpdaterInfo.BundleInfo> pubiList = new List<ProfilerUpdaterInfo.BundleInfo>(updater.TotalCount);
                foreach (UpdateInfo updateInfo in updater.UpdaterBundles)
                {
                    var bundleInfo = ProfilerUpdaterInfo.BundleInfo.Create(updateInfo.Info.BundleIdentifyName,
                        updateInfo.State, updateInfo.Info.Length, updateInfo.DownloadedBytesLength);
                    pubiList.Add(bundleInfo);
                }

                ProfilerUpdaterInfo pui =
                    ProfilerUpdaterInfo.Create(updater.GroupName, updater.State, pubiList,updater.DownloadedBytesLength, updater.Speed);

                info.UpdaterInfoList.Add(pui);
            }
            info.UpdaterInfoList.Sort();
        }

        /// <summary>
        /// 构建分析器对象池信息
        /// </summary>
        private static void BuildProfilerPoolInfo(ProfilerInfo info)
        {
            foreach (var pair in GameObjectPoolManager.PoolDict)
            {
                GameObject go = pair.Key;
                GameObjectPool pool = pair.Value;

                ProfilerPoolInfo ppi = ProfilerPoolInfo.Create(go.name, pool.PoolExpireTime,
                    pool.ObjExpireTime,pool.UnusedTimer,pool.PoolObjectDict.Count);
                info.PoolInfoList.Add(ppi);

                int usedCount = 0;
                foreach (var pair2 in pool.PoolObjectDict)
                {
                   PoolObject poolObject = pair2.Value;

                   ProfilerPoolInfo.PoolObjectInfo poi =
                       ProfilerPoolInfo.PoolObjectInfo.Create(poolObject.Target.GetInstanceID(), poolObject.Used,
                           poolObject.UnusedTimer,
                           poolObject.IsLock);
                   
                   ppi.PoolObjectList.Add(poi);

                   if (poolObject.Used)
                   {
                       usedCount++;
                   }
                }

                ppi.UsedCount = usedCount;
                ppi.UnusedTimer = ppi.AllCount - ppi.UsedCount;
            }
            info.PoolInfoList.Sort();
        }
    }
}