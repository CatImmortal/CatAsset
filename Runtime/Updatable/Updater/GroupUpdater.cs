using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包更新完毕回调的原型
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
        private const ulong generateManifestLength = 1024 * 1024 * 10;  //10M

        /// <summary>
        /// 从上一次重新生成读写区资源清单到现在的字节数 每次资源包更新完毕计算一次
        /// </summary>
        private static ulong deltaUpdatedLength;

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
        private readonly BundleDownloadedCallback onBundleDownloadedCallback;

        /// <summary>
        /// 资源包下载进度刷新回调
        /// </summary>
        private readonly DownloadBundleRefreshCallback onDownloadRefreshCallback;

        /// <summary>
        /// 资源包更新信息 -> 资源包更新完毕回调（外部注册的）
        /// </summary>
        private readonly Dictionary<UpdateInfo, BundleUpdatedCallback> onBundleUpdatedDict =
            new Dictionary<UpdateInfo, BundleUpdatedCallback>();

        /// <summary>
        /// 此更新器的所有资源包集合（待更新的+更新中+已更新的）
        /// </summary>
        private readonly HashSet<UpdateInfo> updaterBundles = new HashSet<UpdateInfo>();

        /// <summary>
        /// 更新中的资源包总数
        /// </summary>
        public int UpdatingCount => GetCount(UpdateState.Updating);

        /// <summary>
        /// 已更新的资源包总数
        /// </summary>
        public int UpdatedCount => GetCount(UpdateState.Updated);

        /// <summary>
        /// 已更新的资源包总长度
        /// </summary>
        public ulong UpdatedLength { get; private set; }

        /// <summary>
        /// 此更新器的资源包总数（待更新的+更新中+已更新的）
        /// </summary>
        public int TotalCount => updaterBundles.Count;

        /// <summary>
        /// 此更新器的资源包总长度（待更新的+更新中+已更新的）
        /// </summary>
        public ulong TotalLength { get; internal set; }

        /// <summary>
        /// 是否已全部更新完毕
        /// </summary>
        public bool IsAllUpdated => UpdatedCount == updaterBundles.Count;

        /// <summary>
        /// 上一次记录的已下载字节数
        /// </summary>
        private ulong lastRecordDownloadBytes;

        /// <summary>
        /// 上一次记录已下载字节数的时间
        /// </summary>
        private float lastRecordTime;

        /// <summary>
        /// 下载速度 单位：字节/秒
        /// </summary>
        public ulong Speed { get; private set; }

        public GroupUpdater()
        {
            onBundleDownloadedCallback = OnBundleDownloaded;
            onDownloadRefreshCallback = OnDownloadUpdate;
        }

        /// <summary>
        /// 添加需要更新的资源包
        /// </summary>
        internal void AddUpdaterBundle(BundleManifestInfo info)
        {
            updaterBundles.Add(new UpdateInfo(info,UpdateState.Wait));
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

            foreach (UpdateInfo updateInfo in updaterBundles)
            {
                if (updateInfo.State == UpdateState.Updated)
                {
                    continue;
                }

                AddUpdatedListener(updateInfo,callback);

                //为了能让优先级变更机制生效 不判断State是否为updating来去重 而是由DownloadBundleTask不处理已合并任务来保证内部不会被重复回调

                //不是更新中的 或者已更新的
                //添加下载文件的任务
                CatAssetManager.AddDownLoadBundleTask(this,updateInfo,onBundleDownloadedCallback,onDownloadRefreshCallback,priority);
                updateInfo.State = UpdateState.Updating;
            }
        }

        /// <summary>
        /// 更新指定的资源包
        /// </summary>
        internal void UpdateBundle(BundleManifestInfo info, BundleUpdatedCallback callback,TaskPriority priority = TaskPriority.VeryHeight)
        {
            foreach (UpdateInfo updateInfo in updaterBundles)
            {
                if (updateInfo.Info.Equals(info))
                {
                    if (updateInfo.State == UpdateState.Updated)
                    {
                        //此资源包已更新
                        callback?.Invoke(new BundleUpdateResult(true,updateInfo,this));
                        return;
                    }

                    State = GroupUpdaterState.Running;

                    //添加回调
                    AddUpdatedListener(updateInfo,callback);

                    //为了能让优先级变更机制生效 不判断State是否为updating来去重 而是由DownloadBundleTask不处理已合并任务来保证内部不会被重复回调
                    CatAssetManager.AddDownLoadBundleTask(this,updateInfo,onBundleDownloadedCallback,onDownloadRefreshCallback,priority);
                    updateInfo.State = UpdateState.Updating;

                    return;
                }
            }
        }

        /// <summary>
        /// 获取指定状态的更新器的数量
        /// </summary>
        private int GetCount(UpdateState state)
        {
            int count = 0;
            foreach (UpdateInfo updateInfo in updaterBundles)
            {
                if (updateInfo.State == state)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 添加资源包更新完毕时的监听回调
        /// </summary>
        private void AddUpdatedListener(UpdateInfo updateInfo,BundleUpdatedCallback callback)
        {
            if (!onBundleUpdatedDict.TryGetValue(updateInfo,out BundleUpdatedCallback value))
            {
                onBundleUpdatedDict.Add(updateInfo,callback);
            }
            else
            {
                value += callback;
                onBundleUpdatedDict[updateInfo] = value;
            }
        }

        /// <summary>
        /// 资源包下载字节数更新的回调
        /// </summary>
        private void OnDownloadUpdate(UpdateInfo updateInfo,ulong deltaDownloadedBytes, ulong totalDownloadedBytes)
        {
            //这里由DownloadBundleTask不处理已合并任务来保证不会被重复回调 一个下载中的资源包只会回调到这里一次
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
        private void OnBundleDownloaded(UpdateInfo updateInfo,bool success)
        {
            //这里由DownloadBundleTask不处理已合并任务来保证不会被重复回调 一个下载完毕的资源包只会回调到这里一次

            updateInfo.State = success ? UpdateState.Updated : UpdateState.Wait;

            //没有资源需要更新了 改变状态为Free
            if (UpdatingCount == 0)
            {
                State = GroupUpdaterState.Free;
                Speed = 0;
                lastRecordTime = 0;
                lastRecordDownloadBytes = 0;
            }

            //取出回调
            onBundleUpdatedDict.TryGetValue(updateInfo, out BundleUpdatedCallback callback);

            BundleUpdateResult result;
            if (!success)
            {
                //下载失败
                Debug.LogError($"更新{updateInfo.Info}失败");

                result = new BundleUpdateResult(false,updateInfo,this);

                if (callback != null)
                {
                    onBundleUpdatedDict.Remove(updateInfo);
                    callback.Invoke(result);
                }

                return;
            }

            //下载成功 刷新已下载资源信息
            deltaUpdatedLength += updateInfo.Info.Length;

            //将下载好的资源包的状态从 InRemote 修改为 InReadWrite，表示可从本地读写区加载
            BundleRuntimeInfo bundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(updateInfo.Info.BundleIdentifyName);
            bundleRuntimeInfo.BundleState = BundleRuntimeInfo.State.InReadWrite;

            //刷新资源组本地资源信息
            GroupInfo groupInfo = CatAssetDatabase.GetOrAddGroupInfo(updateInfo.Info.Group);
            groupInfo.AddLocalBundle(updateInfo.Info.BundleIdentifyName);
            groupInfo.LocalLength += updateInfo.Info.Length;

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
            result = new BundleUpdateResult(true,updateInfo,this);
            if (callback != null)
            {
                onBundleUpdatedDict.Remove(updateInfo);
                callback.Invoke(result);
            }

        }
    }
}
