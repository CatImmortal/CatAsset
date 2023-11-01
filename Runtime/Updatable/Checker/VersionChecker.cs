using System;
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
        /// 是否有读写区资源清单
        /// </summary>
        private static bool hasReadWriteManifest;

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
            string readOnlyManifestPath = RuntimeUtil.GetReadOnlyPath(CatAssetManifest.ManifestBinaryFileName,true);
            string readWriteManifestPath = RuntimeUtil.GetReadWritePath(CatAssetManifest.ManifestBinaryFileName, true);
            string remoteManifestPath = RuntimeUtil.GetRemotePath(CatAssetManifest.ManifestBinaryFileName);


            CatAssetManager.AddWebRequestTask(readOnlyManifestPath, readOnlyManifestPath, CheckReadOnlyManifest,
                TaskPriority.VeryLow);
            CatAssetManager.AddWebRequestTask(readWriteManifestPath, readWriteManifestPath, CheckReadWriteManifest,
                TaskPriority.VeryLow);
            CatAssetManager.AddWebRequestTask(remoteManifestPath, remoteManifestPath, CheckRemoteManifest,
                TaskPriority.VeryLow);

        }

        /// <summary>
        /// 检查只读区资源清单
        /// </summary>
        private static void CheckReadOnlyManifest(bool success, UnityWebRequest uwr)
        {
            if (!success)
            {
                isReadOnlyLoaded = true;
                RefreshCheckInfos();
                Debug.Log($"未加载到只读区资源清单:{uwr.error}");
                return;
            }

            CatAssetManifest manifest = CatAssetManifest.DeserializeFromBinary(uwr.downloadHandler.data);
            
            if (manifest == null)
            {
                string error = "只读区资源清单校验失败";
                ManifestVerifyFailed(error);
                return;
            }
            
            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetOrAddCheckInfo(item.BundleIdentifyName);
                checkInfo.ReadOnlyInfo = item;
            }

            isReadOnlyLoaded = true;
            RefreshCheckInfos();

        }

        /// <summary>
        /// 检查读写区资源清单
        /// </summary>
        private static void CheckReadWriteManifest(bool success, UnityWebRequest uwr)
        {
            hasReadWriteManifest = success;

            if (!success)
            {
                isReadWriteLoaded = true;
                RefreshCheckInfos();
                Debug.Log($"未加载到读写区资源清单：{uwr.error}");
                return;
            }

            CatAssetManifest manifest = CatAssetManifest.DeserializeFromBinary(uwr.downloadHandler.data);
            
            if (manifest == null)
            {
                string error = "读写区资源清单校验失败";
                ManifestVerifyFailed(error);
                return;
            }
            
            foreach (BundleManifestInfo info in manifest.Bundles)
            {
                string path = RuntimeUtil.GetReadWritePath(info.RelativePath);
                bool isVerify = RuntimeUtil.VerifyReadWriteBundle(path,info, true);
                if (!isVerify)
                {
                    //读写区资源清单中记录的资源不能通过校验 就视为其清单信息不存在
                    //防止读写区资源被删除或修改
                    continue;
                }

                CheckInfo checkInfo = GetOrAddCheckInfo(info.BundleIdentifyName);
                checkInfo.ReadWriteInfo = info;
            }

            isReadWriteLoaded = true;
            RefreshCheckInfos();
        }

        /// <summary>
        /// 检查远端资源清单
        /// </summary>
        private static void CheckRemoteManifest(bool success, UnityWebRequest uwr)
        {
            if (!success)
            {
                Debug.LogError($"远端资源清单检查失败:{uwr.error}");
                VersionCheckResult result = new VersionCheckResult(false, uwr.error,0,0);
                onVersionChecked?.Invoke(result);
                Clear();
                return;
            }

            CatAssetManifest manifest = CatAssetManifest.DeserializeFromBinary(uwr.downloadHandler.data);
            
            if (manifest == null)
            {
                string error = "远端资源清单校验失败";
                ManifestVerifyFailed(error);
                return;
            }
            
            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetOrAddCheckInfo(item.BundleIdentifyName);
                checkInfo.RemoteInfo = item;
            }

            isRemoteLoaded = true;
            RefreshCheckInfos();
        }

        /// <summary>
        /// 清单校验失败时调用
        /// </summary>
        private static void ManifestVerifyFailed(string error)
        {
            Debug.LogError(error);
            VersionCheckResult result = new VersionCheckResult(false, error,0,0);
            onVersionChecked?.Invoke(result);
            Clear();
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
            CatAssetDatabase.ClearAllGroupInfo();
            CatAssetUpdater.ClearAllGroupUpdater();

            //需要更新的所有资源包的数量与长度
            int totalCount = 0;
            ulong totalLength = 0;

            bool needGenerateReadWriteManifest = false;

            foreach (KeyValuePair<string,CheckInfo> pair in checkInfoDict)
            {
                CheckInfo checkInfo = pair.Value;
                checkInfo.RefreshState();

                //如果此资源需要更新 并且 不存在读写区资源清单
                if (checkInfo.State == CheckState.NeedUpdate && !hasReadWriteManifest)
                {
                    //可能是读写区资源清单被意外删除了
                    //尝试修复此资源的读写区资源信息
                    if (TryFixReadWriteInfo(checkInfo))
                    {
                        //修复成功 就重新刷新下资源检查状态
                        checkInfo.RefreshState();
                        needGenerateReadWriteManifest = true;
                    }
                }

                if (checkInfo.State != CheckState.Disuse)
                {
                    //添加资源组的远端资源包信息
                    GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(checkInfo.RemoteInfo.Group);
                    groupInfo.AddRemoteBundle(checkInfo.RemoteInfo.BundleIdentifyName);
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
                        groupInfo.AddLocalBundle(checkInfo.RemoteInfo.BundleIdentifyName);
                        groupInfo.LocalLength += checkInfo.RemoteInfo.Length;

                        //添加运行时信息 此资源可从本地读写区加载
                        CatAssetDatabase.InitRuntimeInfo(checkInfo.RemoteInfo,BundleRuntimeInfo.State.InReadWrite);
                        break;

                    case CheckState.InReadOnly:
                        //不需要更新 最新版本存在于只读区
                        groupInfo = CatAssetDatabase.GetOrAddGroupInfo(checkInfo.RemoteInfo.Group);
                        groupInfo.AddLocalBundle(checkInfo.RemoteInfo.BundleIdentifyName);
                        groupInfo.LocalLength += checkInfo.RemoteInfo.Length;

                        //添加运行时信息 此资源可从本地只读区加载
                        CatAssetDatabase.InitRuntimeInfo(checkInfo.RemoteInfo,BundleRuntimeInfo.State.InReadOnly);
                        break;
                }

                if (checkInfo.NeedRemove)
                {
                    //需要删除读写区的那份
                    Debug.Log($"删除读写区资源:{checkInfo.ReadWriteInfo.RelativePath}");
                    string path = RuntimeUtil.GetReadWritePath(checkInfo.ReadWriteInfo.RelativePath);
                    File.Delete(path);

                    needGenerateReadWriteManifest = true;
                }
            }

            if (needGenerateReadWriteManifest)
            {
                //删除过读写区资源 或修复过读写区资源信息 需要重新生成新的读写区资源清单
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            //调用版本检查完毕回调
            VersionCheckResult result = new VersionCheckResult(true, null,totalCount, totalLength);
            onVersionChecked?.Invoke(result);

            Clear();
        }

        /// <summary>
        /// 尝试修复读写区资源信息
        /// </summary>
        private static bool TryFixReadWriteInfo(CheckInfo checkInfo)
        {
            //如果修复过 就要重新生成新的读写区资源清单
            bool needGenerateReadWriteManifest = false;

            if (checkInfo.RemoteInfo != null)
            {
                //没有读写区资源清单信息 尝试修复 防止读写区资源清单被意外删除了
                string path = RuntimeUtil.GetReadWritePath(checkInfo.RemoteInfo.RelativePath);
                if (File.Exists(path))
                {
                    bool isVerify = RuntimeUtil.VerifyReadWriteBundle(path, checkInfo.RemoteInfo);
                    if (isVerify)
                    {
                        checkInfo.ReadWriteInfo = checkInfo.RemoteInfo;
                        needGenerateReadWriteManifest = true;
                        Debug.LogWarning($"修复读写区资源信息:{checkInfo.RemoteInfo.RelativePath}");
                    }
                }
            }

            return needGenerateReadWriteManifest;
        }

        private static void Clear()
        {
            checkInfoDict.Clear();
            onVersionChecked = null;
            isChecking = false;
            isRemoteLoaded = false;
            isReadOnlyLoaded = false;
            isReadWriteLoaded = false;
            hasReadWriteManifest = false;

        }
    }
}
