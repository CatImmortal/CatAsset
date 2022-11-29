﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{

    /// <summary>
    /// 版本检查完毕回调的原型
    /// </summary>
    public delegate void OnVersionChecked(VersionCheckResult result);

    /// <summary>
    /// 版本检查器
    /// </summary>
    public static class VersionChecker
    {

        /// <summary>
        /// 版本检查信息的字典
        /// </summary>
        private static Dictionary<string, CheckInfo> checkInfoDict = new Dictionary<string, CheckInfo>();

        private static OnVersionChecked onVersionChecked;

        private static bool isChecking;

        //三方资源清单的加载完毕标记
        private static bool isReadOnlyLoaded;
        private static bool isReadWriteLoaded;
        private static bool isRemoteLoaded;

        /// <summary>
        /// 检查版本
        /// </summary>
        public static void CheckVersion(OnVersionChecked callback)
        {
            if (isChecking)
            {
                return;
            }
            isChecking = true;

            onVersionChecked = callback;

            //进行只读区 读写区 远端三方的资源清单检查
            string readOnlyManifestPath = RuntimeUtil.GetReadOnlyPath(RuntimeUtil.ManifestFileName);
            string readWriteManifestPath = RuntimeUtil.GetReadWritePath(RuntimeUtil.ManifestFileName,true);
            string remoteManifestPath = RuntimeUtil.GetRemotePath(RuntimeUtil.ManifestFileName);

            CatAssetManager.CheckUpdatableManifest(readOnlyManifestPath,CheckReadOnlyManifest);
            CatAssetManager.CheckUpdatableManifest(readWriteManifestPath,CheckReadWriteManifest);
            CatAssetManager.CheckUpdatableManifest(remoteManifestPath, CheckRemoteManifest);

        }

        /// <summary>
        /// 检查只读区资源清单
        /// </summary>
        private static void CheckReadOnlyManifest(bool success, UnityWebRequest uwr, object userdata)
        {
            if (!success)
            {
                isReadOnlyLoaded = true;
                RefreshCheckInfos();
                Debug.LogWarning($"未加载到只读区资源清单:{uwr.error}");
                return;
            }

            CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);
            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetOrAddCheckInfo(item.RelativePath);
                checkInfo.ReadOnlyInfo = item;
            }

            isReadOnlyLoaded = true;
            RefreshCheckInfos();

        }

        /// <summary>
        /// 检查读写区资源清单
        /// </summary>
        private static void CheckReadWriteManifest(bool success, UnityWebRequest uwr, object userdata)
        {
            if (!success)
            {
                isReadWriteLoaded = true;
                RefreshCheckInfos();
                Debug.LogWarning($"未加载到读写区资源清单：{uwr.error}");
                return;
            }

            CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);
            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetOrAddCheckInfo(item.RelativePath);
                checkInfo.ReadWriteInfo = item;
            }

            isReadWriteLoaded = true;
            RefreshCheckInfos();
        }

        /// <summary>
        /// 检查远端资源清单
        /// </summary>
        private static void CheckRemoteManifest(bool success, UnityWebRequest uwr, object userdata)
        {
            if (!success)
            {
                Debug.LogError($"远端资源清单检查失败:{uwr.error}");
                VersionCheckResult result = new VersionCheckResult(uwr.error,0,0);
                onVersionChecked?.Invoke(result);
                Clear();
                return;
            }

            CatAssetManifest manifest = JsonUtility.FromJson<CatAssetManifest>(uwr.downloadHandler.text);
            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetOrAddCheckInfo(item.RelativePath);
                checkInfo.RemoteInfo = item;
            }

            isRemoteLoaded = true;
            RefreshCheckInfos();
        }

        /// <summary>
        /// 获取资源检查信息，若不存在则添加
        /// </summary>
        private static CheckInfo GetOrAddCheckInfo(string name)
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
        private static void RefreshCheckInfos()
        {
            if (!isReadOnlyLoaded || !isReadWriteLoaded || !isRemoteLoaded)
            {
                //三方资源清单未加载完毕
                return;
            }

            //需要更新的所有资源包的数量与长度
            int totalCount = 0;
            ulong totalLength = 0;

            bool needGenerateReadWriteManifest = false;

            foreach (KeyValuePair<string,CheckInfo> pair in checkInfoDict)
            {
                CheckInfo checkInfo = pair.Value;
                checkInfo.RefreshState();

                if (checkInfo.State != CheckState.Disuse)
                {
                    //添加资源组的远端资源包信息
                    GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(checkInfo.RemoteInfo.Group);
                    groupInfo.AddRemoteBundle(checkInfo.RemoteInfo.RelativePath);
                    groupInfo.RemoteLength += checkInfo.RemoteInfo.Length;
                }

                switch (checkInfo.State)
                {

                    case CheckState.NeedUpdate:
                        //需要更新
                        totalCount++;
                        totalLength += checkInfo.RemoteInfo.Length;

                        //添加至更新器中
                        GroupUpdater groupUpdater = CatAssetUpdater.GetOrAddGroupUpdater(checkInfo.RemoteInfo.Group);
                        groupUpdater.AddUpdaterBundle(checkInfo.RemoteInfo);
                        groupUpdater.TotalLength += checkInfo.RemoteInfo.Length;

                        //添加运行时信息 此资源如果在更新前就被加载了 需要先下载到本地
                        CatAssetDatabase.InitRuntimeInfo(checkInfo.RemoteInfo,BundleRuntimeInfo.State.InRemote);

                        break;

                    case CheckState.InReadWrite:
                        //不需要更新 最新版本存在于读写区
                        GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(checkInfo.RemoteInfo.Group);
                        groupInfo.AddLocalBundle(checkInfo.RemoteInfo.RelativePath);
                        groupInfo.LocalLength += checkInfo.RemoteInfo.Length;

                        //添加运行时信息 此资源可从本地读写区加载
                        CatAssetDatabase.InitRuntimeInfo(checkInfo.RemoteInfo,BundleRuntimeInfo.State.InReadWrite);
                        break;

                    case CheckState.InReadOnly:
                        //不需要更新 最新版本存在于只读区
                        groupInfo = CatAssetDatabase.GetOrAddGroupInfo(checkInfo.RemoteInfo.Group);
                        groupInfo.AddLocalBundle(checkInfo.RemoteInfo.RelativePath);
                        groupInfo.LocalLength += checkInfo.RemoteInfo.Length;

                        //添加运行时信息 此资源可从本地只读区加载
                        CatAssetDatabase.InitRuntimeInfo(checkInfo.RemoteInfo,BundleRuntimeInfo.State.InReadOnly);
                        break;
                }

                if (checkInfo.NeedRemove)
                {
                    //需要删除读写区的那份
                    Debug.Log($"删除读写区资源:{checkInfo.Name}");
                    string path = RuntimeUtil.GetReadWritePath(checkInfo.Name);
                    File.Delete(path);

                    needGenerateReadWriteManifest = true;
                }
            }

            if (needGenerateReadWriteManifest)
            {
                //删除过读写区资源 需要重新生成读写区资源清单
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            //调用版本检查完毕回调
            VersionCheckResult result = new VersionCheckResult(null,totalCount, totalLength);
            onVersionChecked?.Invoke(result);

            Clear();
        }

        private static void Clear()
        {
            checkInfoDict.Clear();
            onVersionChecked = null;
            isChecking = false;
            isRemoteLoaded = false;
            isReadOnlyLoaded = false;
            isReadWriteLoaded = false;

        }
    }
}
