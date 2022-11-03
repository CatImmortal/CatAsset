using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 单个资源包更新完毕回调的原型
    /// </summary>
    public delegate void BundleUpdatedCallback(BundleUpdateResult result);

    /// <summary>
    /// 资源组更新器
    /// </summary>
    public class GroupUpdater
    {
        /// <summary>
        /// 重新生成一次读写区资源清单所需的下载字节数
        /// </summary>
        private const long generateManifestLength = 1024 * 1024 * 10;  //10M

        /// <summary>
        /// 从上一次重新生成读写区资源清单到现在的字节数 每次资源包更新完毕计算一次
        /// </summary>
        private static long deltaUpdatedLength;

        /// <summary>
        /// 资源组名
        /// </summary>
        public string GroupName { get; internal set; }

        /// <summary>
        /// 更新器状态
        /// </summary>
        public GroupUpdaterState State { get; internal set; }

        /// <summary>
        /// 资源包下载结束回调
        /// </summary>
        private DownloadBundleCallback onBundleDownloaded;

        /// <summary>
        /// 资源包下载字节数更新回调
        /// </summary>
        private DownloadBundleUpdateCallback onDownloadUpdate;

        /// <summary>
        /// 单个资源包更新完毕回调(非指定资源包更新)
        /// </summary>
        private BundleUpdatedCallback onBundleUpdated;

        /// <summary>
        /// 资源包 -> 单个资源包更新完毕回调
        /// </summary>
        private Dictionary<BundleManifestInfo, BundleUpdatedCallback> onBundleUpdatedDict =
            new Dictionary<BundleManifestInfo, BundleUpdatedCallback>();

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
        public ulong TotalLength { get; internal set; }

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
        public ulong UpdatedLength { get; private set; }

        /// <summary>
        /// 是否已全部更新完毕
        /// </summary>
        public bool IsAllUpdated => UpdatedCount == TotalCount;

        /// <summary>
        /// 下载速度 单位：字节/秒
        /// </summary>
        public ulong Speed { get; private set; }

        /// <summary>
        /// 上一次记录已下载字节数的时间
        /// </summary>
        private float lastRecordTime;

        /// <summary>
        /// 上一次记录的已下载字节数
        /// </summary>
        private ulong lastRecordDownloadBytes;



        public GroupUpdater()
        {
            onBundleDownloaded = OnBundleDownloaded;
            onDownloadUpdate = OnDownloadUpdate;
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
        internal void UpdateGroup(BundleUpdatedCallback callback,TaskPriority priority = TaskPriority.Middle)
        {
            if (IsAllUpdated)
            {
                //没有资源需要更新
                return;
            }

            State = GroupUpdaterState.Running;
            onBundleUpdated += callback;
            foreach (BundleManifestInfo info in updaterBundles)
            {
                if (updatedBundles.Contains(info))
                {
                    continue;
                }

                //为了能让优先级变更机制生效 不判断是否在updatingBundles中 而是由DownloadBundleTask不处理已合并任务来保证不会重复回调

                //不是更新中的 或者已更新的
                //添加下载文件的任务
                CatAssetManager.AddDownLoadBundleTask(this,info,onBundleDownloaded,onDownloadUpdate,priority);
                updatingBundles.Add(info);
            }
        }

        /// <summary>
        /// 更新指定的资源包
        /// </summary>
        internal void UpdateBundle(BundleManifestInfo info, BundleUpdatedCallback callback,TaskPriority priority = TaskPriority.VeryHeight)
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

            //为了能让优先级变更机制生效 不判断是否在updatingBundles中 而是由DownloadBundleTask不处理已合并任务来保证不会重复回调

            CatAssetManager.AddDownLoadBundleTask(this,info,onBundleDownloaded,onDownloadUpdate,priority);
            updatingBundles.Add(info);

            //添加回调
            if (!onBundleUpdatedDict.TryGetValue(info,out BundleUpdatedCallback value))
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
        /// 资源包下载字节数更新完毕的回调
        /// </summary>
        private void OnDownloadUpdate(ulong deltaDownloadedBytes, ulong totalDownloadedBytes, BundleManifestInfo info)
        {
            //这里由DownloadBundleTask不处理已合并任务来保证不会被重复回调 而是一个下载中的资源包只会回调到这里一次

            UpdatedLength += deltaDownloadedBytes;

            if (lastRecordTime == 0)
            {
                //第一次回调 不计算下载速度
                lastRecordTime = Time.unscaledTime;
                return;
            }

            if (Time.unscaledTime - lastRecordTime > 1)
            {
                //每秒计算一次下载速度
                lastRecordTime = Time.unscaledTime;
                Speed = UpdatedLength - lastRecordDownloadBytes;

                lastRecordDownloadBytes = UpdatedLength;
            }
        }

        /// <summary>
        /// 资源包下载完毕的回调
        /// </summary>
        private void OnBundleDownloaded(bool success, BundleManifestInfo info)
        {
            //这里由DownloadBundleTask不处理已合并任务来保证不会被重复回调 而是一个下载完毕的资源包只会回调到这里一次

            //无论是否下载成功 都要从updatingBundles中移除
            updatingBundles.Remove(info);

            //没有资源需要更新了 改变状态为Free
            if (updatingBundles.Count == 0)
            {
                Speed = UpdatedLength - lastRecordDownloadBytes;
                Debug.Log($"当前下载速度:{RuntimeUtil.GetByteLengthDesc((long)Speed)}");

                State = GroupUpdaterState.Free;
                Speed = 0;
                lastRecordTime = 0;
                lastRecordDownloadBytes = 0;
            }

            onBundleUpdatedDict.TryGetValue(info, out BundleUpdatedCallback callback);

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
            //UpdatedLength += info.Length;
            deltaUpdatedLength += info.Length;

            //将下载好的资源包的状态从 InRemote 修改为 InReadWrite，表示可从本地读写区加载
            BundleRuntimeInfo bundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(info.RelativePath);
            bundleRuntimeInfo.BundleState = BundleRuntimeInfo.State.InReadWrite;

            //刷新读写区资源信息列表
            CatAssetUpdater.AddReadWriteManifestInfo(info);

            //刷新资源组本地资源信息
            GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(info.Group);
            groupInfo.AddLocalBundle(info.RelativePath);
            groupInfo.LocalLength += info.Length;

            if (IsAllUpdated || deltaUpdatedLength >= generateManifestLength)
            {
                //需要更新的资源都更新完了 或者已下载字节数达到要求 就重新生成一次读写区资源清单
                deltaUpdatedLength = 0;
                CatAssetUpdater.GenerateReadWriteManifest();
            }

            if (IsAllUpdated)
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
