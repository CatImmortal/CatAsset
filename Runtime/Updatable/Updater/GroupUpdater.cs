using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包更新回调的原型
    /// </summary>
    public delegate void OnBundleUpdated(BundleUpdateResult result);
    
    /// <summary>
    /// 资源组更新器
    /// </summary>
    public class GroupUpdater
    {
        /// <summary>
        /// 资源组名
        /// </summary>
        public string GroupName { get; internal set; }
        
        /// <summary>
        /// 更新器状态
        /// </summary>
        public GroupUpdaterState State { get; internal set; }
        
        /// <summary>
        /// 需要更新的资源包信息列表
        /// </summary>
        private List<BundleManifestInfo> updateBundles = new List<BundleManifestInfo>();

        /// <summary>
        /// 已回调的资源包总数
        /// </summary>
        private int callbackCount;
        
        /// <summary>
        /// 已更新的资源包总数
        /// </summary>
        public int UpdatedCount { get; private set; }

        /// <summary>
        /// 已更新的资源包总长度
        /// </summary>
        public long UpdatedLength { get; private set; }
        
        /// <summary>
        /// 需要更新的资源包总数
        /// </summary>
        public int TotalCount { get; internal set; }
        
        /// <summary>
        /// 需要更新的资源包总长度
        /// </summary>
        public long TotalLength { get; internal set; }

        /// <summary>
        /// 重新生成一次读写区资源清单所需的下载字节数
        /// </summary>
        private const long generateManifestLength = 1024 * 1024 * 10;  //10M
        
        /// <summary>
        /// 从上一次重新生成读写区资源清单到现在下载的字节数
        /// </summary>
        private long deltaUpdatedLength;

        /// <summary>
        /// 资源包更新回调
        /// </summary>
        private OnBundleUpdated onBundleUpdated;

        /// <summary>
        /// 资源包下载回调
        /// </summary>
        private DownloadBundleCallback onBundleDownloaded;

        public GroupUpdater()
        {
            onBundleDownloaded = OnBundleDownloaded;
        }

        /// <summary>
        /// 添加需要更新的资源包信息
        /// </summary>
        internal void AddUpdateBundle(BundleManifestInfo info)
        {
            updateBundles.Add(info);
        }

        /// <summary>
        /// 更新资源组
        /// </summary>
        internal void UpdateGroup(OnBundleUpdated callback)
        {
            if (State != GroupUpdaterState.Free)
            {
                //非空闲状态 不处理
                return;
            }
            
            State = GroupUpdaterState.Running;
            foreach (BundleManifestInfo info in updateBundles)
            {
                //创建下载文件的任务
                string localFilePath = Util.GetReadWritePath(info.RelativePath);
                string downloadUri = Path.Combine(CatAssetUpdater.UpdateUriPrefix, info.RelativePath);
                CatAssetManager.DownloadBundle(this,info,downloadUri,localFilePath,onBundleDownloaded);
            }
            
            onBundleUpdated = callback;
        }
        
        /// <summary>
        /// 资源包下载完毕的回调
        /// </summary>
        private void OnBundleDownloaded(bool success, BundleManifestInfo info)
        {
            callbackCount++;
            if (callbackCount == TotalCount)
            {
                //所有需要下载的资源包都回调过 就将状态改为Free
                //若此时有下载失败的资源包，导致UpdatedCount < TotalCount，则可通过重新启动此Updater来进行下载
                State = GroupUpdaterState.Free;
            }

            BundleUpdateResult result;
            if (!success)
            {
                Debug.LogError($"更新{info.RelativePath}失败");
                result = new BundleUpdateResult(false,info.RelativePath,this);
                onBundleUpdated?.Invoke(result);
                return;
            }

            updateBundles.Remove(info);
            
            //刷新已下载资源信息
            UpdatedCount++;
            UpdatedLength += info.Length;
            deltaUpdatedLength += info.Length;
            
            //将下载好的资源包的运行时信息添加到CatAssetDatabase中
            CatAssetDatabase.InitRuntimeInfo(info, true);
            
            //刷新读写区资源信息列表
            CatAssetUpdater.AddReadWriteManifestInfo(info);
            
            //刷新资源组本地资源信息
            GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(info.Group);
            groupInfo.AddLocalBundle(info.RelativePath);
            groupInfo.LocalCount++;
            groupInfo.LocalLength += info.Length;
            
            bool allDownloaded = UpdatedCount >= TotalCount;
            if (allDownloaded || deltaUpdatedLength >= generateManifestLength)
            {
                //资源下载完毕 或者已下载字节数达到要求 就重新生成一次读写区资源清单
                deltaUpdatedLength = 0;
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            if (allDownloaded)
            {
                //该组资源都更新完毕，可以删掉updater了
                CatAssetUpdater.RemoveGroupUpdater(GroupName);
            }
            
            //调用外部回调
            result = new BundleUpdateResult(true,info.RelativePath,this);
            onBundleUpdated?.Invoke(result);
        }
    }
}