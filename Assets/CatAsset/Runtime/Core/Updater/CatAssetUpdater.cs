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
    internal static class CatAssetUpdater
    {
        /// <summary>
        /// 读写区资源信息，用于生成读写区资源清单
        /// </summary>
        internal static Dictionary<string, AssetBundleManifestInfo> readWriteManifestInfoDict = new Dictionary<string, AssetBundleManifestInfo>();

        /// <summary>
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/AssetBundleName 为下载地址
        /// </summary>
        internal static string UpdateUriPrefix;

        /// <summary>
        /// 资源更新器字典 key为资源组
        /// </summary>
        internal static Dictionary<string, Updater> updaterDict = new Dictionary<string, Updater>();

        /// <summary>
        /// 资源更新器（没指定资源组，更新所有资源）
        /// </summary>
        internal static Updater updater;

        /// <summary>
        /// 生成读写区资源清单
        /// </summary>
        internal static void GenerateReadWriteManifest()
        {
            CatAssetManifest manifest = new CatAssetManifest();
            manifest.GameVersion = Application.version;
            manifest.ManifestVersion = 0;
            AssetBundleManifestInfo[] abInfos = new AssetBundleManifestInfo[readWriteManifestInfoDict.Count];
            int index = 0;
            foreach (KeyValuePair<string, AssetBundleManifestInfo> item in readWriteManifestInfoDict)
            {
                abInfos[index] = item.Value;
                index++;
            }

            manifest.AssetBundles = abInfos;

            //写入清单文件json
            string json = JsonParser.ToJson(manifest);
            string path = Util.GetReadWritePath(Util.GetManifestFileName());
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
        /// 资源版本信息检查
        /// </summary>
        internal static void CheckVersion(Action<int, long,string> onVersionChecked,string checkGroup)
        {
            Checker checker = new Checker();
            checker.CheckVersion(onVersionChecked, checkGroup);
        }


        /// <summary>
        /// 更新资源
        /// </summary>
        internal static void UpdateAsset(Action<int, long, int, long, string, string> onFileDownloaded,string updateGroup)
        {
            if (string.IsNullOrEmpty(updateGroup) && updater != null)
            {
                //更新所有资源
                updater.UpdateAsset(onFileDownloaded);
                return;
            }

            if (updaterDict.TryGetValue(updateGroup,out Updater groupUpdater))
            {
                //更新指定资源组
                groupUpdater.UpdateAsset(onFileDownloaded);
                return;
            }

            if (updateGroup == null)
            {
                Debug.LogError("没有找到资源更新器");
            }
            else
            {
                Debug.LogError("没有找到该资源组的资源更新器：" + updateGroup);
            }

           
        }

    }
}

