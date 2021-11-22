using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 资源更新器
    /// </summary>
    public class Updater
    {
       

        /// <summary>
        /// 重新生成一次读写区资源清单所需的下载字节数
        /// </summary>
        private static long generateManifestLength = 1024 * 1024 * 10;  //10M

        /// <summary>
        /// 从上一次重新生成读写区资源清单到现在下载的字节数
        /// </summary>
        private long deltaUpatedLength;

        /// <summary>
        /// Bundle更新回调
        /// </summary>
        private Action<bool, int, long, int, long, string, string> onUpdated;

        /// <summary>
        /// 文件下载回调
        /// </summary>
        private Action<bool, BundleManifestInfo> onDownloadFinished;

        /// <summary>
        /// 正在更新中的资源
        /// </summary>
        private HashSet<string> UpdatingBundles = new HashSet<string>();

        /// <summary>
        /// 更新器状态
        /// </summary>
        public UpdaterStatus state;

        /// <summary>
        /// 需要更新的资源
        /// </summary>
        public Dictionary<string,BundleManifestInfo> UpdateBundles = new Dictionary<string, BundleManifestInfo>();

        /// <summary>
        /// 需要更新的资源组
        /// </summary>
        public string UpdateGroup;

        /// <summary>
        /// 需要更新的资源总数
        /// </summary>
        public int TotalCount;

        /// <summary>
        /// 需要更新的资源长度
        /// </summary>
        public long TotalLength;

        /// <summary>
        /// 已更新资源文件数量
        /// </summary>
        public int UpdatedCount;

        /// <summary>
        /// 已更新资源文件长度
        /// </summary>
        public long UpdatedLength;


        public Updater()
        {
            onDownloadFinished = OnDownloadFinished;
        }

        /// <summary>
        /// 移除更新完毕回调，主要给边玩边下模式调用
        /// </summary>
        internal void RemoveBundleUpdatedCallback(Action<bool, int, long, int, long, string, string> onUpdated)
        {
            this.onUpdated -= onUpdated;
        }

        /// <summary>
        /// 更新指定资源，主要给边玩边下模式调用
        /// </summary>
        internal void UpdateAsset(string name, Action<bool, int, long, int, long, string, string> onUpdated)
        {
            BundleManifestInfo updateBundleInfo = UpdateBundles[name];

            AddUpdateTask(updateBundleInfo);

            this.onUpdated += onUpdated;
        }

        /// <summary>
        /// 更新所有需要更新的资源文件
        /// </summary>
        internal void UpdateAssets(Action<bool,int, long, int, long, string, string> onUpdated)
        {
            state = UpdaterStatus.Runing;

            foreach (KeyValuePair<string, BundleManifestInfo> item in UpdateBundles)
            {
                //创建下载文件的任务
                AddUpdateTask(item.Value);
            }

            this.onUpdated += onUpdated;
        }

        /// <summary>
        /// 添加更新任务，已在更新中就不添加了
        /// </summary>
        private void AddUpdateTask(BundleManifestInfo updateBundleInfo)
        {
            if (UpdatingBundles.Contains(updateBundleInfo.BundleName))
            {
                //已在更新中，不处理了
                return;
            }

            string localFilePath = Util.GetReadWritePath(updateBundleInfo.BundleName);
            string downloadUri = Path.Combine(CatAssetUpdater.UpdateUriPrefix, updateBundleInfo.BundleName);
            DownloadFileTask task = new DownloadFileTask(CatAssetManager.taskExcutor, downloadUri, updateBundleInfo, this, localFilePath, downloadUri, onDownloadFinished);
            CatAssetManager.taskExcutor.AddTask(task);

            //记录下来
            UpdatingBundles.Add(updateBundleInfo.BundleName);
        }

        /// <summary>
        /// 资源文件下载完毕的回调
        /// </summary>
        private void OnDownloadFinished(bool success, BundleManifestInfo bundleInfo)
        {

            UpdatingBundles.Remove(bundleInfo.BundleName); //从正在更新中的资源集合中移除

            if (!success)
            {
                Debug.LogError($"更新{bundleInfo.BundleName}失败");
                onUpdated?.Invoke(false, UpdatedCount, UpdatedLength, TotalCount, TotalLength, bundleInfo.BundleName, UpdateGroup);
                return;
            }


            //刷新已下载资源信息
            UpdateBundles.Remove(bundleInfo.BundleName);  //从需要更新的资源集合中移除
            UpdatedCount++;
            UpdatedLength += bundleInfo.Length;
            deltaUpatedLength += bundleInfo.Length;

            //将下载好的bundle信息添加到RuntimeInfo中
            CatAssetManager.InitRuntimeInfo(bundleInfo, true);

            //刷新读写区资源信息列表
            CatAssetUpdater.readWriteManifestInfoDict[bundleInfo.BundleName] = bundleInfo;

            //刷新资源组本地资源信息
            GroupInfo groupInfo = CatAssetManager.GetOrCreateGroupInfo(bundleInfo.Group);
            groupInfo.localBundles.Add(bundleInfo.BundleName);
            groupInfo.localCount++;
            groupInfo.localLength += bundleInfo.Length;

            bool allDownloaded = UpdatedCount >= TotalCount;

            if (allDownloaded || deltaUpatedLength >= generateManifestLength)
            {
                //资源下载完毕 或者已下载字节数达到要求 就重新生成一次读写区资源清单
                deltaUpatedLength = 0;
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            if (allDownloaded)
            {
                //该组资源都更新完毕，可以删掉updater了
                CatAssetUpdater.groupUpdaterDict.Remove(UpdateGroup);
            }

            onUpdated?.Invoke(true, UpdatedCount, UpdatedLength,TotalCount,TotalLength,bundleInfo.BundleName,UpdateGroup);
        }
    }

}

