using System.Collections.Generic;
using System.IO;

namespace CatAsset.Runtime
{
    public static partial class CatAssetManager
    {
        /// <summary>
        /// 获取资源组信息
        /// </summary>
        public static GroupInfo GetGroupInfo(string group)
        {
            return CatAssetDatabase.GetGroupInfo(group);
        }

        /// <summary>
        /// 获取所有资源组信息
        /// </summary>
        public static List<GroupInfo> GetAllGroupInfo()
        {
            return CatAssetDatabase.GetAllGroupInfo();
        }

        /// <summary>
        /// 获取指定资源组的更新器
        /// </summary>
        public static GroupUpdater GetGroupUpdater(string group)
        {
            return CatAssetUpdater.GetGroupUpdater(group);
        }
        
        /// <summary>
        /// 更新资源组
        /// </summary>
        public static void UpdateGroup(string group, BundleUpdatedCallback callback)
        {
            CatAssetUpdater.UpdateGroup(group, callback);
        }
        
        /// <summary>
        /// 更新指定的资源包
        /// </summary>
        public static void UpdateBundle(string group, BundleManifestInfo info, BundleUpdatedCallback callback,
            TaskPriority priority = TaskPriority.VeryHeight)
        {
            CatAssetUpdater.UpdateBundle(group,info,callback,priority);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        public static void PauseGroupUpdater(string group)
        {
            CatAssetUpdater.PauseGroupUpdate(group, true);
        }
        
        /// <summary>
        /// 恢复资源组更新
        /// </summary>
        public static void ResumeGroupUpdater(string group)
        {
            CatAssetUpdater.PauseGroupUpdate(group, false);
        }

        /// <summary>
        /// 添加资源包下载任务
        /// </summary>
        internal static void AddDownLoadBundleTask(GroupUpdater updater, BundleManifestInfo info,
            BundleDownloadedCallback onBundleDownloadedCallback, DownloadBundleUpdateCallback onDownloadUpdateCallback,
            TaskPriority priority = TaskPriority.Middle)
        {
            string localFilePath = RuntimeUtil.GetReadWritePath(info.RelativePath);
            string downloadUri =
                RuntimeUtil.GetRegularPath(Path.Combine(CatAssetUpdater.UpdateUriPrefix, info.RelativePath));

            DownloadBundleTask task =
                DownloadBundleTask.Create(downloadTaskRunner, downloadUri, info, updater, downloadUri,
                    localFilePath, onBundleDownloadedCallback, onDownloadUpdateCallback);
            downloadTaskRunner.AddTask(task, priority);
        }


    }
}