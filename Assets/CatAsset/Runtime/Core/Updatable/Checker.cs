using CatJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset
{
    /// <summary>
    /// 资源检查器
    /// </summary>
    public class Checker
    {
        /// <summary>
        /// 资源检查信息的字典
        /// </summary>
        private Dictionary<string, CheckInfo> checkInfoDict = new Dictionary<string, CheckInfo>();

        /// <summary>
        /// 版本信息检查完毕回调
        /// </summary>
        private Action<int, long> onVersionChecked;


        //三方资源清单的检查完毕标记
        private bool readOnlyChecked;
        private bool readWriteCheked;
        private bool remoteChecked;

        /// <summary>
        /// 检查资源版本
        /// </summary>
        public void CheckVersion(Action<int, long> onVersionChecked)
        {

            this.onVersionChecked = onVersionChecked;

            //进行只读区 读写区 远端三方的资源清单检查
            string readOnlyManifestPath = Util.GetReadOnlyPath(Util.ManifestFileName);
            WebRequestTask task1 = new WebRequestTask(CatAssetManager.taskExcutor, readOnlyManifestPath, readOnlyManifestPath, CheckReadOnlyManifest);

            string readWriteManifestPath = Util.GetReadWritePath(Util.ManifestFileName);
            WebRequestTask task2 = new WebRequestTask(CatAssetManager.taskExcutor, readWriteManifestPath, readWriteManifestPath, CheckReadWriteManifest);

            string remoteManifestUri = Path.Combine(CatAssetUpdater.UpdateUriPrefix, Util.ManifestFileName);
            WebRequestTask task3 = new WebRequestTask(CatAssetManager.taskExcutor, remoteManifestUri, remoteManifestUri, CheckRemoteManifest);

            CatAssetManager.taskExcutor.AddTask(task1);
            CatAssetManager.taskExcutor.AddTask(task2);
            CatAssetManager.taskExcutor.AddTask(task3);
        }

        /// <summary>
        /// 检查只读区资源清单
        /// </summary>
        private void CheckReadOnlyManifest(bool success, string error, UnityWebRequest uwr)
        {
            if (!success)
            {
                readOnlyChecked = true;
                RefershCheckInfos();
                return;
            }

            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetCheckInfo(item.BundleName);
                checkInfo.ReadOnlyInfo = item;
            }

            readOnlyChecked = true;
            RefershCheckInfos();
        }

        /// <summary>
        /// 检查读写区资源清单
        /// </summary>
        private void CheckReadWriteManifest(bool success, string error, UnityWebRequest uwr)
        {
            if (!success)
            {
                readWriteCheked = true;
                RefershCheckInfos();
                return;
            }

            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetCheckInfo(item.BundleName);
                checkInfo.ReadWriteInfo = item;
                CatAssetUpdater.readWriteManifestInfoDict[item.BundleName] = item;
            }

            readWriteCheked = true;
            RefershCheckInfos();
        }

        /// <summary>
        /// 检查远端资源清单
        /// </summary>
        private void CheckRemoteManifest(bool success, string error, UnityWebRequest uwr)
        {
            if (!success)
            {
                Debug.LogError("远端资源清单检查失败:" + error);
                return;
            }

            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetCheckInfo(item.BundleName);
                checkInfo.RemoteInfo = item;
            }

            if (CatAssetManager.RunMode == RunMode.UpdatableWhilePlaying)
            {
                //边玩边下模式 需要初始化远端资源清单信息
                CatAssetManager.InitRemoteManifestInfo(manifest);
            }

            remoteChecked = true;
            RefershCheckInfos();
        }

        /// <summary>
        /// 获取资源检查信息
        /// </summary>
        private CheckInfo GetCheckInfo(string name)
        {
            if (!checkInfoDict.TryGetValue(name, out CheckInfo checkInfo))
            {
                checkInfo = new CheckInfo(name);
                checkInfoDict.Add(name, checkInfo);
                return checkInfo;
            }

            return checkInfo;
        }



        /// <summary>
        /// 刷新资源检查信息
        /// </summary>
        private void RefershCheckInfos()
        {
            if (!readOnlyChecked || !readWriteCheked || !remoteChecked)
            {
                return;
            }

            //清理旧的资源组与更新器信息
            CatAssetManager.groupInfoDict.Clear();
            CatAssetUpdater.groupUpdaterDict.Clear();

            //需要更新的所有资源的数量与长度
            int totalCount = 0;
            long totalLength = 0;
            

            //是否需要生成读写区资源清单
            bool needGenerateManifest = false;

            foreach (KeyValuePair<string, CheckInfo> item in checkInfoDict)
            {
                CheckInfo checkInfo = item.Value;
                checkInfo.RefreshState();

                switch (checkInfo.State)
                {
                    case CheckStatus.NeedUpdate:
                        //需要更新

                        Updater updater = CatAssetUpdater.GetOrCreateGroupUpdater(checkInfo.RemoteInfo.Group);
                        updater.UpdateBundles.Add(item.Key,checkInfo.RemoteInfo);
                        updater.TotalCount++;
                        updater.TotalLength += checkInfo.RemoteInfo.Length;

                        totalCount++;
                        totalLength += checkInfo.RemoteInfo.Length;

                        break;

                    case CheckStatus.InReadWrite:
                        //最新版本已存放在读写区
                        CatAssetManager.InitRuntimeInfo(checkInfo.ReadWriteInfo, true);
                        break;

                    case CheckStatus.InReadOnly:
                        //最新版本已存放在只读区
                        CatAssetManager.InitRuntimeInfo(checkInfo.ReadOnlyInfo, false);
                        break;
                }

                if (checkInfo.NeedRemove)
                {
                    //需要删除
                    Debug.Log("删除读写区资源：" + checkInfo.Name);
                    string path = Util.GetReadWritePath(checkInfo.Name);
                    File.Delete(path);

                    //从读写区资源信息字典中删除
                    CatAssetUpdater.readWriteManifestInfoDict.Remove(checkInfo.Name);

                    needGenerateManifest = true;
                }
            }

            if (needGenerateManifest)
            {
                //删除过读写区资源 需要重新生成读写区资源清单
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            //调用版本信息检查完毕回调
            onVersionChecked?.Invoke(totalCount, totalLength);

        }

    }
}

