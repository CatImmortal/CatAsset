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
        private static Dictionary<string, AssetBundleCheckInfo> checkInfoDict = new Dictionary<string, AssetBundleCheckInfo>();

        private static Action<int, long> checkVersionCompleted;

        private static bool RemoteChecked;
        private static bool ReadOnlyChecked;

        private static Action<int, long> updateCallback;
        private static int updatedCount;
        private static long updatedLength;

        /// <summary>
        /// 需要更新的资源列表
        /// </summary>
        public static List<string> needUpdateList = new List<string>();

        /// <summary>
        /// 资源更新Uri前缀
        /// </summary>
        public static string UpdateUriPrefix
        {
            get;
            set;
        }

        /// <summary>
        /// 版本信息检查
        /// </summary>
        public static void CheckVersion(Action<int, long> checkVersionCompleted)
        {
            CatAssetUpdater.checkVersionCompleted = checkVersionCompleted;

            //进行远端 只读区 的资源清单检查
            string remoteManifestUri = Path.Combine(UpdateUriPrefix, Util.GetManifestName());
       
            WebRequestTask task1 = new WebRequestTask(CatAssetManager.taskExcutor, remoteManifestUri, 0, CheckRemoteManifest, null);


            string readOnlyManifestPath = Util.GetReadOnlyPath(Util.GetManifestName());
            WebRequestTask task2 = new WebRequestTask(CatAssetManager.taskExcutor, readOnlyManifestPath, 0, CheckReadOnlyManifest, null);

            CatAssetManager.taskExcutor.AddTask(task1);
            CatAssetManager.taskExcutor.AddTask(task2);
        }

        /// <summary>
        /// 获取资源检查信息
        /// </summary>
        private static AssetBundleCheckInfo GetCheckInfo(string name)
        {
            if (!checkInfoDict.TryGetValue(name, out AssetBundleCheckInfo checkInfo))
            {
                checkInfo = new AssetBundleCheckInfo(name);
                checkInfoDict.Add(name, checkInfo);
                return checkInfo;
            }

            return checkInfo;
        }

        /// <summary>
        /// 检查远端资源清单
        /// </summary>
        private static void CheckRemoteManifest(object obj)
        {
            if (obj == null)
            {
                Debug.LogError("远端资源清单检查失败");
                return;
            }

            UnityWebRequest uwr = (UnityWebRequest)obj;
            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                AssetBundleCheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.RemoteInfo = item;
            }

            RemoteChecked = true;
            UpdateCheckInfos();
        }

        /// <summary>
        /// 检查只读区资源清单
        /// </summary>
        private static void CheckReadOnlyManifest(object obj)
        {
            if (obj == null)
            {
                ReadOnlyChecked = true;
                UpdateCheckInfos();
                return;
            }

            UnityWebRequest uwr = (UnityWebRequest)obj;
            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                AssetBundleCheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.ReadOnlyInfo = item;
            }

            ReadOnlyChecked = true;
            UpdateCheckInfos();
        }
        
        /// <summary>
        /// 检查读写区资源文件
        /// </summary>
        private static void CheckReadWriteAssetBundles()
        {
            //todo:
        }

        /// <summary>
        /// 刷新资源检查信息
        /// </summary>
        private static void UpdateCheckInfos()
        {
            if (!RemoteChecked || !ReadOnlyChecked)
            {
                return;
            }

            //需要更新的资源数量
            int updateCount = 0;

            //需要更新的资源大小
            long updateTotalLength = 0;

            foreach (KeyValuePair<string, AssetBundleCheckInfo> item in checkInfoDict)
            {
                AssetBundleCheckInfo checkInfo = item.Value;
                checkInfo.UpdateState();

                if (checkInfo.State == CheckState.NeedUpdate)
                {
                    //需要更新
                    updateCount++;
                    updateTotalLength += checkInfo.RemoteInfo.Length;
                    needUpdateList.Add(checkInfo.Name);
                }
                else if (checkInfo.NeedRemove)
                {
                    //需要删除
                    Debug.Log("删除读写区资源：" + checkInfo.Name);
                    string path = Util.GetReadWritePath(checkInfo.Name);
                    File.Delete(path);
                }
            }

            //调用版本信息检查完毕回调
            checkVersionCompleted(updateCount, updateTotalLength);
        }

        /// <summary>
        /// 更新资源
        /// </summary>
        public static void UpdateAsset(Action<int,long> updateCallback)
        {
            CatAssetUpdater.updateCallback = updateCallback;

            foreach (string name in needUpdateList)
            {
                string downloadUri = Path.Combine(UpdateUriPrefix, name);
                DownloadFileTask task = new DownloadFileTask(CatAssetManager.taskExcutor, name, 0, DownloadCompleted, downloadUri);
                CatAssetManager.taskExcutor.AddTask(task);
            }
        }

        private static void DownloadCompleted(object obj)
        {
            if (obj == null)
            {
                return;
            }

            updatedCount++;
            updatedLength += ((long)(obj as UnityWebRequest).downloadedBytes);

            updateCallback(updatedCount, updatedLength);
        }
    }
}

