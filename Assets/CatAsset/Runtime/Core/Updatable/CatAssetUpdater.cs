using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using CatJson;
using System.IO;
using System;

namespace CatAsset
{
    /// <summary>
    /// CatAsset资源更新器
    /// </summary>
    public static class CatAssetUpdater
    {
        
        /// <summary>
        /// 读写区资源信息，用于生成读写区资源清单
        /// </summary>
        internal static Dictionary<string, BundleManifestInfo> readWriteManifestInfoDict = new Dictionary<string, BundleManifestInfo>();

        /// <summary>
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/AssetBundleName 为下载地址
        /// </summary>
        internal static string UpdateUriPrefix;

        /// <summary>
        /// 资源更新器字典 key为资源组
        /// </summary>
        internal static Dictionary<string, Updater> groupUpdaterDict = new Dictionary<string, Updater>();

        /// <summary>
        /// 生成读写区资源清单
        /// </summary>
        internal static void GenerateReadWriteManifest()
        {
            CatAssetManifest manifest = new CatAssetManifest();
            manifest.GameVersion = Application.version;
            BundleManifestInfo[] bundleInfos = new BundleManifestInfo[readWriteManifestInfoDict.Count];
            int index = 0;
            foreach (KeyValuePair<string, BundleManifestInfo> item in readWriteManifestInfoDict)
            {
                bundleInfos[index] = item.Value;
                index++;
            }

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
        /// 检查资源版本
        /// </summary>
        internal static void CheckVersion(Action<int, long> onVersionChecked)
        {
            Checker checker = new Checker();
            checker.CheckVersion(onVersionChecked);
        }

        /// <summary>
        /// 获取指定资源组的更新器，若不存在则创建
        /// </summary>
        internal static Updater GetOrCreateGroupUpdater(string group)
        {
            if (!groupUpdaterDict.TryGetValue(group,out Updater updater))
            {
                updater = new Updater();
                updater.UpdateGroup = group;
                groupUpdaterDict.Add(group, updater);
            }
            return updater;
        }

        /// <summary>
        /// 更新资源
        /// </summary>
        internal static void UpdateAssets(Action<bool,int, long, int, long, string, string> onUpdated,string updateGroup)
        {
            if (!groupUpdaterDict.TryGetValue(updateGroup,out Updater groupUpdater))
            {
                Debug.LogError("更新失败，没有找到该资源组的资源更新器：" + updateGroup);
                return;
            }

            //更新指定资源组
            if (groupUpdater.state != UpdaterStatus.Free)
            {
                Debug.LogError("此资源组已在更新中：" + updateGroup);
            }
            else
            {
                groupUpdater.UpdateAssets(onUpdated);
            }
        }

        /// <summary>
        /// 暂停资源更新器
        /// </summary>
        internal static void PauseUpdater(bool isPause ,string group)
        {
            if (!groupUpdaterDict.TryGetValue(group, out Updater groupUpdater))
            {
                Debug.LogError("暂停失败，没有找到该资源组的资源更新器：" + group);
                return;
            }

            //暂停指定资源组的更新器
            if (groupUpdater.state == UpdaterStatus.Runing)
            {
                groupUpdater.state = UpdaterStatus.Paused;
            }

          
        }

    }
}

