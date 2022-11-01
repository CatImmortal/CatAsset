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
        /// 资源包下载任务回调
        /// </summary>
        private DownloadBundleCallback onBundleDownloaded;
        
        /// <summary>
        /// 资源包更新回调(非指定资源包更新)
        /// </summary>
        private OnBundleUpdated onBundleUpdated;

        /// <summary>
        /// 资源包 -> 对应资源包更新回调
        /// </summary>
        private Dictionary<BundleManifestInfo, OnBundleUpdated> onBundleUpdatedDict =
            new Dictionary<BundleManifestInfo, OnBundleUpdated>();

        
        /// <summary>
        /// 此更新器的资源包集合（包含待更新的+已更新的）
        /// </summary>
        private HashSet<BundleManifestInfo> updaterBundles = new HashSet<BundleManifestInfo>();

        /// <summary>
        /// 此更新器的资源包总数（包含待更新的+已更新的）
        /// </summary>
        public int TotalCount => updaterBundles.Count;
        
        /// <summary>
        /// 此更新器的资源包总长度（包含待更新的+已更新的）
        /// </summary>
        public long TotalLength { get; internal set; }

        /// <summary>
        /// 更新中的资源包集合
        /// </summary>
        private HashSet<BundleManifestInfo> updatingBundles = new HashSet<BundleManifestInfo>();

        /// <summary>
        /// 已更新的资源包集合
        /// </summary>
        private HashSet<BundleManifestInfo> updatedBundles = new HashSet<BundleManifestInfo>();
        
        /// <summary>
        /// 已更新的资源包总数
        /// </summary>
        public int UpdatedCount => updatedBundles.Count;

        /// <summary>
        /// 已更新的资源包总长度
        /// </summary>
        public long UpdatedLength { get; private set; }

        
        /// <summary>
        /// 重新生成一次读写区资源清单所需的下载字节数
        /// </summary>
        private const long generateManifestLength = 1024 * 1024 * 10;  //10M
        
        /// <summary>
        /// 从上一次重新生成读写区资源清单到现在下载的字节数
        /// </summary>
        private long deltaUpdatedLength;



        public GroupUpdater()
        {
            onBundleDownloaded = OnBundleDownloaded;
        }

        /// <summary>
        /// 添加资源包信息
        /// </summary>
        internal void AddUpdaterBundle(BundleManifestInfo info)
        {
            updaterBundles.Add(info);
        }

        /// <summary>
        /// 更新所有待更新资源包
        /// </summary>
        internal void UpdateGroup(OnBundleUpdated callback,TaskPriority priority = TaskPriority.Middle)
        {
            if (updatingBundles.Count + updatedBundles.Count  == updaterBundles.Count)
            {
                //没有资源需要更新
                return;
            }
            
            State = GroupUpdaterState.Running;
            onBundleUpdated += callback;
            foreach (BundleManifestInfo info in updaterBundles)
            {
                if (!updatingBundles.Contains(info) && !updatedBundles.Contains(info))
                {
                    //不是更新中的 或者已更新的
                    //添加下载文件的任务
                    CatAssetManager.AddDownLoadBundleTask(this,info,onBundleDownloaded,priority);
                    updatingBundles.Add(info);
                }
            }
        }
        
        /// <summary>
        /// 更新指定的资源包
        /// </summary>
        internal void UpdateBundle(BundleManifestInfo info, OnBundleUpdated callback,TaskPriority priority = TaskPriority.VeryHeight)
        {
            if (!updaterBundles.Contains(info))
            {
                //此更新器没有此资源包
                return;
            }

            if (updatedBundles.Contains(info))
            {
                //此资源包已更新
                return;
            }

            if (!updatingBundles.Contains(info))
            {
                CatAssetManager.AddDownLoadBundleTask(this,info,onBundleDownloaded,priority);
                updatingBundles.Add(info);
            }
            
            //添加回调
            if (!onBundleUpdatedDict.TryGetValue(info,out OnBundleUpdated value))
            {
                onBundleUpdatedDict.Add(info,callback);
            }
            else
            {
                value += callback;
                onBundleUpdatedDict[info] = value;
            }
           
           
        }
        
        /// <summary>
        /// 资源包下载完毕的回调
        /// </summary>
        private void OnBundleDownloaded(bool success, BundleManifestInfo info)
        {
            //无论是否下载成功 都要从updatingBundles中移除
            updatingBundles.Remove(info);

            //没有资源需要下载了 改变状态为Free
            if (updatingBundles.Count == 0)
            {
                State = GroupUpdaterState.Free;
            }
            
            onBundleUpdatedDict.TryGetValue(info, out OnBundleUpdated callback);
            
            BundleUpdateResult result;
            if (!success)
            {
                //下载失败
                Debug.LogError($"更新{info.RelativePath}失败");
                
                result = new BundleUpdateResult(false,info.RelativePath,this);
                onBundleUpdated?.Invoke(result);
                if (State == GroupUpdaterState.Free)
                {
                    onBundleUpdated = null;
                }
                
                if (callback != null)
                {
                    onBundleUpdatedDict.Remove(info);
                    callback.Invoke(result);
                }
                
                return;
            }

            //下载成功 刷新已下载资源信息
            updatedBundles.Add(info);
            UpdatedLength += info.Length;
            deltaUpdatedLength += info.Length;
            
            //将下载好的资源包的运行时信息添加到CatAssetDatabase中
            CatAssetDatabase.InitRuntimeInfo(info, true);
            
            //刷新读写区资源信息列表
            CatAssetUpdater.AddReadWriteManifestInfo(info);
            
            //刷新资源组本地资源信息
            GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(info.Group);
            groupInfo.AddLocalBundle(info.RelativePath);
            groupInfo.LocalLength += info.Length;
            
            if (updatingBundles.Count == 0 || deltaUpdatedLength >= generateManifestLength)
            {
                //没有资源需要下载了 或者已下载字节数达到要求 就重新生成一次读写区资源清单
                deltaUpdatedLength = 0;
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            if (UpdatedCount == TotalCount)
            {
                //该组资源都更新完毕，可以删掉updater了
                CatAssetUpdater.RemoveGroupUpdater(GroupName);
            }

            //调用外部回调
            result = new BundleUpdateResult(true,info.RelativePath,this);
            onBundleUpdated?.Invoke(result);
            if (State == GroupUpdaterState.Free)
            {
                onBundleUpdated = null;
            }
            
            if (callback != null)
            {
                onBundleUpdatedDict.Remove(info);
                callback.Invoke(result);
            }

        }
    }
}