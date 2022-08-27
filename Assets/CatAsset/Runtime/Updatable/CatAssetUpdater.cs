using System.Collections.Generic;
using System.IO;
using CatJson;
using UnityEngine;
namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源更新器
    /// </summary>
    public static class CatAssetUpdater
    {
        /// <summary>
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/BundleRelativePath 为下载地址
        /// </summary>
        internal static string UpdateUriPrefix;
        
        /// <summary>
        /// 资源包相对路径->读写区资源包清单信息，用于生成读写区资源清单
        /// </summary>
        private static Dictionary<string, BundleManifestInfo> readWriteManifestInfoDict = new Dictionary<string, BundleManifestInfo>();

        /// <summary>
        /// 资源组名->资源组对应的资源更新器
        /// </summary>
        private static Dictionary<string, GroupUpdater> groupUpdaterDict = new Dictionary<string, GroupUpdater>();

        /// <summary>
        /// 添加读写区资源包清单信息
        /// </summary>
        internal static void AddReadWriteManifestInfo(BundleManifestInfo info)
        {
            readWriteManifestInfoDict[info.RelativePath] = info;
        }
        
        /// <summary>
        /// 移除读写区资源包清单信息
        /// </summary>
        internal static void RemoveReadWriteManifestInfo(BundleManifestInfo info)
        {
            readWriteManifestInfoDict.Remove(info.RelativePath);
        }
        
        /// <summary>
        /// 清空读写区资源包清单信息
        /// </summary>
        internal static void ClearReadWriteManifestInfo()
        {
            readWriteManifestInfoDict.Clear();
        }
        
        /// <summary>
        /// 生成读写区资源清单
        /// </summary>
        internal static void GenerateReadWriteManifest()
        {
            //创建 CatAssetManifest
            CatAssetManifest manifest = new CatAssetManifest();
            manifest.GameVersion = Application.version;
            List<BundleManifestInfo> bundleInfos = new List<BundleManifestInfo>(readWriteManifestInfoDict.Count);
            foreach (KeyValuePair<string, BundleManifestInfo> item in readWriteManifestInfoDict)
            {
                bundleInfos.Add(item.Value);
            }
            bundleInfos.Sort();
            manifest.Bundles = bundleInfos;

            //写入清单文件json
            string json = JsonParser.ToJson(manifest);
            string path = Util.GetReadWritePath(Util.ManifestFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(json);
            }
        }
        
        /// <summary>
        /// 获取指定资源组的更新器，若不存在则添加
        /// </summary>
        internal static GroupUpdater GetOrAddGroupUpdater(string group)
        {
            if (!groupUpdaterDict.TryGetValue(group,out GroupUpdater updater))
            {
                updater = new GroupUpdater();
                updater.GroupName = group;
                groupUpdaterDict.Add(group, updater);
            }
            return updater;
        }

        /// <summary>
        /// 获取指定资源组的更新器
        /// </summary>
        internal static GroupUpdater GetGroupUpdater(string group)
        {
            groupUpdaterDict.TryGetValue(group, out GroupUpdater updater);
            return updater;
        }

        /// <summary>
        /// 删除指定资源组的更新器
        /// </summary>
        internal static void RemoveGroupUpdater(string group)
        {
            groupUpdaterDict.Remove(group);
        }

        /// <summary>
        /// 更新资源组
        /// </summary>
        internal static void UpdateGroup(string group,OnBundleUpdated callback)
        {
            if (!groupUpdaterDict.TryGetValue(group,out GroupUpdater groupUpdater))
            {
                Debug.LogError($"更新失败，没有找到该资源组的资源更新器：{group}");
                return;
            }
            
            //更新指定资源组
            groupUpdater.UpdateGroup(callback);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        internal static void PauseGroupUpdate(string group, bool isPause)
        {
            if (!groupUpdaterDict.TryGetValue(group,out GroupUpdater groupUpdater))
            {
                Debug.LogError($"暂停失败，没有找到该资源组的更新器：{group}");
                return;
            }

            //暂停指定资源组更新器
            if (isPause)
            {
                if (groupUpdater.State == GroupUpdaterState.Running)
                {
                    groupUpdater.State = GroupUpdaterState.Paused;
                }
            }
            else
            {
                if (groupUpdater.State == GroupUpdaterState.Paused)
                {
                    groupUpdater.State = GroupUpdaterState.Running;
                }
            }
        }
    }
}