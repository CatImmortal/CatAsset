using System;
using System.Collections.Generic;
using System.IO;
using CatJson;
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
            string readOnlyManifestPath = Util.GetReadOnlyPath(Util.ManifestFileName);
            string readWriteManifestPath = Util.GetReadWritePath(Util.ManifestFileName);
            string remoteManifestPath = Util.GetRemotePath(Util.ManifestFileName);
            
            CatAssetManager.CheckUpdatableManifest(readOnlyManifestPath,CheckReadOnlyManifest);
            CatAssetManager.CheckUpdatableManifest(readWriteManifestPath,CheckReadWriteManifest);
            CatAssetManager.CheckUpdatableManifest(remoteManifestPath, CheckRemoteManifest);
            
        }

        /// <summary>
        /// 检查只读区区资源清单
        /// </summary>
        private static void CheckReadOnlyManifest(bool success, UnityWebRequest uwr, object userdata)
        {
            if (!success)
            {
                isReadOnlyLoaded = true;
                RefreshCheckInfos();
                return;
            }
            
            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

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
                return;
            }
            
            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

            foreach (BundleManifestInfo item in manifest.Bundles)
            {
                CheckInfo checkInfo = GetOrAddCheckInfo(item.RelativePath);
                checkInfo.ReadWriteInfo = item;
                CatAssetUpdater.AddReadWriteManifestInfo(item);
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
                VersionCheckResult result = new VersionCheckResult(uwr.error,default,default);
                onVersionChecked?.Invoke(result);
                Clear();
                
                return;
            }
            
            CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(uwr.downloadHandler.text);

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
            long totalLength = 0;
            
            //是否重新需要生成读写区资源清单
            bool needGenerateManifest = false;

            foreach (KeyValuePair<string,CheckInfo> pair in checkInfoDict)
            {
                CheckInfo checkInfo = pair.Value;
                checkInfo.RefreshState();

                switch (checkInfo.State)
                {

                    case CheckState.NeedUpdate:
                        //需要更新
                        totalCount++;
                        totalLength += checkInfo.RemoteInfo.Length;
                        break;
                    
                    case CheckState.InReadWrite:
                        //不需要更新 最新版本存在于读写区
                        CatAssetDatabase.InitRuntimeInfo(checkInfo.ReadWriteInfo,true);
                        break;
                    
                    case CheckState.InReadOnly:
                        //不需要更新 最新版本存在于只读区
                        CatAssetDatabase.InitRuntimeInfo(checkInfo.ReadOnlyInfo,false);
                        break;
                }

                if (checkInfo.NeedRemove)
                {
                    //需要删除读写区的那份
                    Debug.Log($"删除读写区资源:{checkInfo.Name}");
                    string path = Util.GetReadWritePath(checkInfo.Name);
                    File.Delete(path);
                    CatAssetUpdater.RemoveReadWriteManifestInfo(checkInfo.ReadWriteInfo);
                    
                    //删除过读写区资源 需要重新生成
                    needGenerateManifest = true;

                }
            }

            if (needGenerateManifest)
            {
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