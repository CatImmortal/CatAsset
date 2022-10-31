using System.Collections.Generic;

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
        public static void UpdateGroup(string group, OnBundleUpdated callback)
        {
            CatAssetUpdater.UpdateGroup(group, callback);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        public static void PauseGroupUpdater(string group, bool isPause)
        {
            CatAssetUpdater.PauseGroupUpdate(group, isPause);
        }

        /// <summary>
        /// 添加资源包下载任务
        /// </summary>
        internal static void AddDownloadBundleTask(GroupUpdater groupUpdater, BundleManifestInfo info, string downloadUri,
            string localFilePath, DownloadBundleCallback callback,TaskPriority priority = TaskPriority.Middle)
        {
            DownloadBundleTask task =
                DownloadBundleTask.Create(downloadTaskRunner, downloadUri, info, groupUpdater, downloadUri,
                    localFilePath, callback);
            downloadTaskRunner.AddTask(task, priority);
        }
        

    }
}