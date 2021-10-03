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
    /// 资源更新器
    /// </summary>
    public static class CatAssetUpdater
    {
        /// <summary>
        /// 资源检查信息的字典
        /// </summary>
        private static Dictionary<string, UpdateCheckInfo> checkInfoDict = new Dictionary<string, UpdateCheckInfo>();

        /// <summary>
        /// 读写区资源信息
        /// </summary>
        private static Dictionary<string, AssetBundleManifestInfo> readWriteManifestInfoDict = new Dictionary<string, AssetBundleManifestInfo>();

        /// <summary>
        /// 版本信息检查完毕回调
        /// </summary>
        private static Action<int, long> onVersionChecked;

        //三方资源清单的检查完毕标记
        private static bool readOnlyChecked;
        private static bool readWriteCheked;
        private static bool remoteChecked;

        /// <summary>
        /// 需要更新的资源列表
        /// </summary>
        private static List<AssetBundleManifestInfo> needUpdateList = new List<AssetBundleManifestInfo>();

        /// <summary>
        /// 需要删除的资源列表
        /// </summary>
        private static List<string> needRemoveList = new List<string>();

        /// <summary>
        /// 资源文件更新回调，每次下载资源文件后调用
        /// </summary>
        private static Action<int, long> onFileDownloaded;

        /// <summary>
        /// 已更新资源文件数量
        /// </summary>
        private static int updatedCount;

        /// <summary>
        /// 已更新资源文件长度
        /// </summary>
        private static long updatedLength;

        /// <summary>
        /// 重新生成一次读写区资源清单所需的下载字节数
        /// </summary>
        private static long generateManifestLength = 1024 * 1024 * 10;  //10M

        /// <summary>
        /// 从上一次重新生成读写区资源清单到现在下载的字节数
        /// </summary>
        private static long deltaUpatedLength;

        /// <summary>
        /// 资源更新Uri前缀，下载资源文件时会以 UpdateUriPrefix/AssetBundleName 为下载地址
        /// </summary>
        internal static string UpdateUriPrefix;

        /// <summary>
        /// 资源版本信息检查
        /// </summary>
        internal static void CheckVersion(Action<int, long> onVersionChecked)
        {
            CatAssetUpdater.onVersionChecked = onVersionChecked;

            //进行只读区 读写区 远端三方的资源清单检查
            string readOnlyManifestPath = Util.GetReadOnlyPath(Util.GetManifestFileName());
            WebRequestTask task1 = new WebRequestTask(CatAssetManager.taskExcutor, readOnlyManifestPath, readOnlyManifestPath, CheckReadOnlyManifest);
            
            string readWriteManifestPath = Util.GetReadWritePath(Util.GetManifestFileName());
            WebRequestTask task2 = new WebRequestTask(CatAssetManager.taskExcutor, readWriteManifestPath,readWriteManifestPath, CheckReadWriteManifest);

            string remoteManifestUri = Path.Combine(UpdateUriPrefix, Util.GetManifestFileName());
            WebRequestTask task3 = new WebRequestTask(CatAssetManager.taskExcutor, remoteManifestUri, remoteManifestUri, CheckRemoteManifest);

            CatAssetManager.taskExcutor.AddTask(task1);
            CatAssetManager.taskExcutor.AddTask(task2);
            CatAssetManager.taskExcutor.AddTask(task3);
        }

        /// <summary>
        /// 检查只读区资源清单
        /// </summary>
        private static void CheckReadOnlyManifest(bool success,string error,UnityWebRequest uwr)
        {
            if (!success)
            {
                readOnlyChecked = true;
                RefershCheckInfos();
                return;
            }

            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                UpdateCheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.ReadOnlyInfo = item;
            }

            readOnlyChecked = true;
            RefershCheckInfos();
        }

        /// <summary>
        /// 检查读写区资源清单
        /// </summary>
        private static void CheckReadWriteManifest(bool success, string error, UnityWebRequest uwr)
        {
            if (!success)
            {
                readWriteCheked = true;
                RefershCheckInfos();
                return;
            }

            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                UpdateCheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.ReadWriteInfo = item;

                readWriteManifestInfoDict.Add(item.AssetBundleName, item);
            }

            readWriteCheked = true;
            RefershCheckInfos();
        }

        /// <summary>
        /// 检查远端资源清单
        /// </summary>
        private static void CheckRemoteManifest(bool success, string error, UnityWebRequest uwr)
        {
            if (!success)
            {
                Debug.LogError("远端资源清单检查失败:" + error);
                return;
            }

            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                UpdateCheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.RemoteInfo = item;
            }

            remoteChecked = true;
            RefershCheckInfos();
        }

        /// <summary>
        /// 获取资源检查信息
        /// </summary>
        private static UpdateCheckInfo GetCheckInfo(string name)
        {
            if (!checkInfoDict.TryGetValue(name, out UpdateCheckInfo checkInfo))
            {
                checkInfo = new UpdateCheckInfo(name);
                checkInfoDict.Add(name, checkInfo);
                return checkInfo;
            }

            return checkInfo;
        }

        /// <summary>
        /// 刷新资源检查信息
        /// </summary>
        private static void RefershCheckInfos()
        {
            if (!readOnlyChecked || !readWriteCheked || !remoteChecked)
            {
                return;
            }

            //需要更新的资源总数
            int updateTotalCount = 0;

            //需要更新的资源大小
            long updateTotalLength = 0;

            //是否需要生成读写区资源清单
            bool needGenerateManifest = false;

            foreach (KeyValuePair<string, UpdateCheckInfo> item in checkInfoDict)
            {
                UpdateCheckInfo checkInfo = item.Value;
                checkInfo.RefreshState();

                switch (checkInfo.State)
                {
                    case UpdateCheckState.NeedUpdate:
                        //需要更新
                        updateTotalCount++;
                        updateTotalLength += checkInfo.RemoteInfo.Length;
                        needUpdateList.Add(checkInfo.RemoteInfo);
                        break;

                    case UpdateCheckState.InReadWrite:
                        //最新版本已存放在读写区
                        CatAssetManager.AddRuntimeInfo(checkInfo.ReadWriteInfo,true);
                        break;

                    case UpdateCheckState.InReadOnly:
                        //最新版本已存放在只读区
                        CatAssetManager.AddRuntimeInfo(checkInfo.ReadOnlyInfo, false);
                        break;
                }

                if (checkInfo.NeedRemove)
                {
                    //需要删除
                    Debug.Log("删除读写区资源：" + checkInfo.Name);
                    string path = Util.GetReadWritePath(checkInfo.Name);
                    File.Delete(path);

                    //从读写区资源信息字典中删除
                    readWriteManifestInfoDict.Remove(checkInfo.Name);

                    needGenerateManifest = true;
                }
            }

            if (needGenerateManifest)
            {
                //删除过读写区资源 需要重新生成读写区资源清单
                GenerateReadWriteManifest();
            }

            //调用版本信息检查完毕回调
            onVersionChecked(updateTotalCount, updateTotalLength);

        }

        /// <summary>
        /// 生成读写区资源清单
        /// </summary>
        private static void GenerateReadWriteManifest()
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
        /// 更新资源
        /// </summary>
        internal static void UpdateAssets(Action<int,long> onFileDownloaded)
        {
            
            CatAssetUpdater.onFileDownloaded = onFileDownloaded;

            foreach (AssetBundleManifestInfo updateABInfo in needUpdateList)
            {
                string localFilePath = Util.GetReadWritePath(updateABInfo.AssetBundleName);
                string downloadUri = Path.Combine(UpdateUriPrefix, updateABInfo.AssetBundleName);
                DownloadFileTask task = new DownloadFileTask(CatAssetManager.taskExcutor, downloadUri,updateABInfo, localFilePath, downloadUri, OnDownloadFinished);
                CatAssetManager.taskExcutor.AddTask(task);
            }
        }

        /// <summary>
        /// 资源文件下载回调
        /// </summary>
        private static void OnDownloadFinished(bool success, string error,object userData)
        {
            AssetBundleManifestInfo abInfo = (AssetBundleManifestInfo)userData;

            if (!success)
            {
                Debug.LogError($"下载文件{abInfo.AssetBundleName}失败：" + error);
                return;
            }

            //将下载好的ab信息添加到RuntimeInfo中
            CatAssetManager.AddRuntimeInfo(abInfo, true);

            //更新读写区资源信息列表
            readWriteManifestInfoDict[abInfo.AssetBundleName] = abInfo;

            updatedCount++;
            updatedLength += abInfo.Length;
            deltaUpatedLength += abInfo.Length;

            if (updatedCount >= needUpdateList.Count || deltaUpatedLength >= generateManifestLength)
            {
                //所有资源下载完毕 或者已下载字节数达到要求 就重新生成一次读写区资源清单
                deltaUpatedLength = 0;
                GenerateReadWriteManifest();
            }

            onFileDownloaded(updatedCount, updatedLength);
        }
    }
}

