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
        private Action<int, long, string> onVersionChecked;

        /// <summary>
        /// 检查的资源组
        /// </summary>
        private string checkGroup;

        //三方资源清单的检查完毕标记
        private bool readOnlyChecked;
        private bool readWriteCheked;
        private bool remoteChecked;

        /// <summary>
        /// 资源版本信息检查
        /// </summary>
        public void CheckVersion(Action<int, long, string> onVersionChecked, string checkGroup)
        {

            this.onVersionChecked = onVersionChecked;
            this.checkGroup = checkGroup;

            //进行只读区 读写区 远端三方的资源清单检查
            string readOnlyManifestPath = Util.GetReadOnlyPath(Util.GetManifestFileName());
            WebRequestTask task1 = new WebRequestTask(CatAssetManager.taskExcutor, readOnlyManifestPath, readOnlyManifestPath, CheckReadOnlyManifest);

            string readWriteManifestPath = Util.GetReadWritePath(Util.GetManifestFileName());
            WebRequestTask task2 = new WebRequestTask(CatAssetManager.taskExcutor, readWriteManifestPath, readWriteManifestPath, CheckReadWriteManifest);

            string remoteManifestUri = Path.Combine(CatAssetUpdater.UpdateUriPrefix, Util.GetManifestFileName());
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

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                if (!string.IsNullOrEmpty(checkGroup) && item.Group != checkGroup)
                {
                    continue;
                }

                CheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
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

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                if (!string.IsNullOrEmpty(checkGroup) && item.Group != checkGroup)
                {
                    continue;
                }

                CheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.ReadWriteInfo = item;
                CatAssetUpdater.readWriteManifestInfoDict[item.AssetBundleName] = item;
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

            foreach (AssetBundleManifestInfo item in manifest.AssetBundles)
            {
                if (!string.IsNullOrEmpty(checkGroup) && item.Group != checkGroup)
                {
                    continue;
                }

                CheckInfo checkInfo = GetCheckInfo(item.AssetBundleName);
                checkInfo.RemoteInfo = item;
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

            Updater updater = new Updater();
            updater.UpdateGroup = checkGroup;

            //是否需要生成读写区资源清单
            bool needGenerateManifest = false;

            foreach (KeyValuePair<string, CheckInfo> item in checkInfoDict)
            {
                CheckInfo checkInfo = item.Value;
                checkInfo.RefreshState();

                switch (checkInfo.State)
                {
                    case CheckState.NeedUpdate:
                        //需要更新
                        updater.UpdateList.Add(checkInfo.RemoteInfo);
                        updater.totalCount++;
                        updater.totalLength += checkInfo.RemoteInfo.Length;
                        break;

                    case CheckState.InReadWrite:
                        //最新版本已存放在读写区
                        CatAssetManager.AddRuntimeInfo(checkInfo.ReadWriteInfo, true);
                        break;

                    case CheckState.InReadOnly:
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
                    CatAssetUpdater.readWriteManifestInfoDict.Remove(checkInfo.Name);

                    needGenerateManifest = true;
                }
            }

            if (needGenerateManifest)
            {
                //删除过读写区资源 需要重新生成读写区资源清单
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            //有指定资源组的就放进字典里 没指定的就放到字段里
            if (!string.IsNullOrEmpty(checkGroup))
            {
                CatAssetUpdater.updaterDict[checkGroup] = updater;
            }
            else
            {
                CatAssetUpdater.updater = updater;
            }

            //调用版本信息检查完毕回调
            onVersionChecked(updater.totalCount, updater.totalLength, checkGroup);

        }

    }
}

