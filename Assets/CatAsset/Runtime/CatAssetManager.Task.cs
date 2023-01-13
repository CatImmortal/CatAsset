using System;
using System.IO;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 添加Web请求任务
        /// </summary>
        public static void AddWebRequestTask(string name, string uri, WebRequestedCallback callback,TaskPriority priority)
        {
            WebRequestTask task = WebRequestTask.Create(loadTaskRunner, name, uri, callback);
            loadTaskRunner.AddTask(task, priority);
        }
        
        /// <summary>
        /// 添加加载内置资源包资源的任务
        /// </summary>
        public static void AddLoadBundledAssetTask(string assetName,Type assetType,AssetHandler handler,TaskPriority priority)
        {
            LoadBundledAssetTask loadBundledAssetTask =
                LoadBundledAssetTask.Create(loadTaskRunner, assetName, assetType, handler);
            loadTaskRunner.AddTask(loadBundledAssetTask, priority);

            handler.Task = loadBundledAssetTask;
        }
        
        /// <summary>
        /// 添加加载原生资源的任务
        /// </summary>
        public static void AddLoadRawAssetTask(string assetName,AssetCategory category,AssetHandler handler,TaskPriority priority)
        {
            LoadRawAssetTask loadRawAssetTask =
                LoadRawAssetTask.Create(loadTaskRunner, assetName, category, handler);
            loadTaskRunner.AddTask(loadRawAssetTask, priority);
            
            handler.Task = loadRawAssetTask;
        }
        
        /// <summary>
        /// 添加资源包下载任务
        /// </summary>
        internal static void AddDownLoadBundleTask(GroupUpdater updater, UpdateInfo updateInfo,
            BundleDownloadedCallback onBundleDownloadedCallback, DownloadBundleRefreshCallback onDownloadRefreshCallback,
            TaskPriority priority = TaskPriority.Middle)
        {
            string localFilePath = RuntimeUtil.GetReadWritePath(updateInfo.Info.RelativePath);
            string downloadUri =
                RuntimeUtil.GetRegularPath(Path.Combine(CatAssetUpdater.UpdateUriPrefix, updateInfo.Info.RelativePath));

            DownloadBundleTask task =
                DownloadBundleTask.Create(downloadTaskRunner, downloadUri, updateInfo, updater, downloadUri,
                    localFilePath, onBundleDownloadedCallback, onDownloadRefreshCallback);
            downloadTaskRunner.AddTask(task, priority);
        }
    }
}