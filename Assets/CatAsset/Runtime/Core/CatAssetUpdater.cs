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

        /// <summary>
        /// 读写区资源信息
        /// </summary>
        private static Dictionary<string, AssetBundleManifestInfo> readWriteManifestInfoDict = new Dictionary<string, AssetBundleManifestInfo>();

        private static Action<int, long> checkVersionCompleted;

        private static bool readOnlyChecked;
        private static bool readWriteCheked;
        private static bool remoteChecked;

        private static Action<int, long> updateCallback;
        private static int updatedCount;
        private static long updatedLength;

        /// <summary>
        /// 重新生成一次读写区资源清单所需的下载字节数
        /// </summary>
        //private static long generateManifestLength = 1024 * 1024 * 10;  //10M
        private static long generateManifestLength = 0;

        /// <summary>
        /// 从上一次重新生成读写区资源清单到现在下载的字节数
        /// </summary>
        private static long deltaUpatedLength;

        /// <summary>
        /// 需要更新的资源列表
        /// </summary>
        public static List<AssetBundleManifestInfo> needUpdateList = new List<AssetBundleManifestInfo>();

        /// <summary>
        /// 资源更新Uri前缀
        /// </summary>
        public static string UpdateUriPrefix
        {
            get;
            set;
        }

        /// <summary>
        /// 资源版本信息检查
        /// </summary>
        public static void CheckVersion(Action<int, long> checkVersionCompleted)
        {
            CatAssetUpdater.checkVersionCompleted = checkVersionCompleted;

            //进行只读区 读写区 远端三方的资源清单检查
            string readOnlyManifestPath = Util.GetReadOnlyPath(Util.GetManifestFileName());
            WebRequestTask task1 = new WebRequestTask(CatAssetManager.taskExcutor, readOnlyManifestPath, 0, CheckReadOnlyManifest, null);

            string readWriteManifestPath = Util.GetReadWritePath(Util.GetManifestFileName());
            WebRequestTask task2 = new WebRequestTask(CatAssetManager.taskExcutor, readWriteManifestPath, 0, CheckReadWriteManifest, null);

            string remoteManifestUri = Path.Combine(UpdateUriPrefix, Util.GetManifestFileName());
            WebRequestTask task3 = new WebRequestTask(CatAssetManager.taskExcutor, remoteManifestUri, 0, CheckRemoteManifest, null);

            CatAssetManager.taskExcutor.AddTask(task1);
            CatAssetManager.taskExcutor.AddTask(task2);
            CatAssetManager.taskExcutor.AddTask(task3);
        }

        /// <summary>
        /// 检查只读区资源清单
        /// </summary>
        private static void CheckReadOnlyManifest(object obj)
        {
            if (obj == null)
            {
                readOnlyChecked = true;
                RefershCheckInfos();
                return;
            }

            UnityWebRequest uwr = (UnityWebRequest)obj;
            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                AssetBundleCheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.ReadOnlyInfo = item;
            }

            readOnlyChecked = true;
            RefershCheckInfos();
        }

        /// <summary>
        /// 检查读写区资源清单
        /// </summary>
        private static void CheckReadWriteManifest(object obj)
        {
            if (obj == null)
            {
                readWriteCheked = true;
                RefershCheckInfos();
                return;
            }

            UnityWebRequest uwr = (UnityWebRequest)obj;
            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                AssetBundleCheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.ReadWriteInfo = item;

                readWriteManifestInfoDict.Add(item.AssetBundleName, item);
            }

            readWriteCheked = true;
            RefershCheckInfos();
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

            remoteChecked = true;
            RefershCheckInfos();
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

            foreach (KeyValuePair<string, AssetBundleCheckInfo> item in checkInfoDict)
            {
                AssetBundleCheckInfo checkInfo = item.Value;
                checkInfo.RefreshState();

                switch (checkInfo.State)
                {
                    case CheckState.NeedUpdate:

                        //需要更新
                        updateTotalCount++;
                        updateTotalLength += checkInfo.RemoteInfo.Length;
                        needUpdateList.Add(checkInfo.RemoteInfo);

                        break;

                    case CheckState.InReadWrite:
                        CatAssetManager.AddAssetBundleRuntimeInfo(checkInfo.ReadWriteInfo,true);
                        break;

                    case CheckState.InReadOnly:
                        CatAssetManager.AddAssetBundleRuntimeInfo(checkInfo.ReadOnlyInfo, false);
                        break;

                    case CheckState.Disuse:

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

                        break;

                    default:
                        break;
                }


            }

            
            if (needGenerateManifest)
            {
                //删除过读写区资源 需要重新生成读写区资源清单
                GenerateReadWriteManifest();
            }

            //调用版本信息检查完毕回调
            checkVersionCompleted(updateTotalCount, updateTotalLength);
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
        public static void UpdateAssets(Action<int,long> updateCallback)
        {
            if (needUpdateList.Count == 0)
            {
                updateCallback(0, 0);
            }


            CatAssetUpdater.updateCallback = updateCallback;

            foreach (AssetBundleManifestInfo updateABInfo in needUpdateList)
            {
                string localFilePath = Util.GetReadWritePath(updateABInfo.AssetBundleName);
                string downloadUri = Path.Combine(UpdateUriPrefix, updateABInfo.AssetBundleName);
                DownloadFileTask task = new DownloadFileTask(CatAssetManager.taskExcutor, updateABInfo.AssetBundleName, 0, OnDownloadCompleted,localFilePath,downloadUri,updateABInfo);
                CatAssetManager.taskExcutor.AddTask(task);
            }
        }

        private static void OnDownloadCompleted(object userData)
        {
            if (userData == null)
            {
                return;
            }

            //将下载好的ab信息添加到RuntimeInfo中
            AssetBundleManifestInfo abInfo = (AssetBundleManifestInfo)userData;
            CatAssetManager.AddAssetBundleRuntimeInfo(abInfo, true);

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

            updateCallback(updatedCount, updatedLength);
        }
    }
}

