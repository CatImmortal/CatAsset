using System;
using System.IO;
using System.Threading;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 添加Web请求任务
        /// </summary>
        public static void AddWebRequestTask(string name, string uri, WebRequestedCallback callback,
            TaskPriority priority)
        {
            WebRequestTask task = WebRequestTask.Create(loadTaskRunner, name, uri, callback);
            loadTaskRunner.AddTask(task, priority);
        }

        /// <summary>
        /// 添加加载内置资源包资源的任务
        /// </summary>
        internal static void AddLoadAssetTask(string assetName, Type assetType, AssetHandler handler,
            CancellationToken token, TaskPriority priority)
        {
            LoadAssetTask loadAssetTask =
                LoadAssetTask.Create(loadTaskRunner, assetName, assetType, handler, token);
            loadTaskRunner.AddTask(loadAssetTask, priority);

            handler.Task = loadAssetTask;
        }

        /// <summary>
        /// 添加加载场景的任务
        /// </summary>
        public static void AddLoadSceneTask(string sceneName, SceneHandler handler, CancellationToken token,
            TaskPriority priority)
        {
            LoadSceneTask task = LoadSceneTask.Create(loadTaskRunner, sceneName, handler, token);
            loadTaskRunner.AddTask(task, priority);

            handler.Task = task;
        }

        /// <summary>
        /// 添加加载原生资源的任务
        /// </summary>
        internal static void AddLoadRawAssetTask(string assetName, AssetCategory category, AssetHandler handler,
            CancellationToken token, TaskPriority priority)
        {
            LoadRawAssetTask loadRawAssetTask =
                LoadRawAssetTask.Create(loadTaskRunner, assetName, category, handler, token);
            loadTaskRunner.AddTask(loadRawAssetTask, priority);

            handler.Task = loadRawAssetTask;
        }

        /// <summary>
        /// 添加资源包下载任务
        /// </summary>
        internal static void AddDownLoadBundleTask(GroupUpdater updater, UpdateInfo updateInfo,
            BundleDownloadedCallback onBundleDownloadedCallback,
            DownloadBundleRefreshCallback onDownloadRefreshCallback,
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