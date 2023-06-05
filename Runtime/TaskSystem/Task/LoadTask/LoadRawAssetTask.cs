using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 原生资源加载任务
    /// </summary>
    public partial class LoadRawAssetTask : BaseTask
    {
        /// <summary>
        /// 原生资源加载状态
        /// </summary>
        private enum LoadRawAssetState
        {
            None,
            
            /// <summary>
            /// 不存在于本地
            /// </summary>
            NotExist,

            /// <summary>
            /// 下载中
            /// </summary>
            Downloading,

            /// <summary>
            /// 下载结束
            /// </summary>
            Downloaded,

            /// <summary>
            /// 资源未加载
            /// </summary>
            NotLoad,
            
            /// <summary>
            /// 资源加载中
            /// </summary>
            Loading,

            /// <summary>
            /// 资源加载结束
            /// </summary>
            Loaded,
        }

        private AssetCategory category;
        private AssetHandler handler;

        private AssetRuntimeInfo assetRuntimeInfo;
        private BundleRuntimeInfo bundleRuntimeInfo;
        private LoadRawAssetState loadState;

        private readonly BundleUpdatedCallback onRawAssetUpdatedCallback;
        
        private WebRequestTask webRequestTask;
        private readonly WebRequestedCallback onWebRequestedCallback;

        private float startLoadTime;
        
        /// <inheritdoc />
        public override string SubState => loadState.ToString();
        
        /// <inheritdoc />
        public override float Progress
        {
            get
            {
                if (webRequestTask == null)
                {
                    return 0;
                }

                return webRequestTask.Progress;
            }
        }
        
        public LoadRawAssetTask()
        {
            onRawAssetUpdatedCallback = OnRawAssetUpdated;
            onWebRequestedCallback = OnWebRequested;
        }
        

        /// <inheritdoc />
        public override void Run()
        {
            startLoadTime = Time.realtimeSinceStartup;
            if (bundleRuntimeInfo.BundleState == BundleRuntimeInfo.State.InRemote)
            {
                //不在本地 需要先下载
                loadState = LoadRawAssetState.NotExist;
            }
            else
            {
                //在本地了
                if (assetRuntimeInfo.Asset == null)
                {
                    //未加载
                    loadState = LoadRawAssetState.NotLoad;
                }
                else
                {
                    //已加载好
                    loadState = LoadRawAssetState.Loaded;
                }
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            CheckAllCanceled();

            if (loadState == LoadRawAssetState.NotExist)
            {
                //不存在本地
                CheckStateWhileNotExist();
            }
            
            if (loadState == LoadRawAssetState.Downloading)
            {
                //下载中
                CheckStateWhileDownloading();
            }
            
            if (loadState == LoadRawAssetState.Downloaded)
            {
                //下载结束
                CheckStateWhileDownloaded();
            }
            
            if (loadState == LoadRawAssetState.NotLoad)
            {
                //未加载
                CheckStateWhileNotLoad();
            }

            if (loadState == LoadRawAssetState.Loading)
            {
                //加载中
                CheckStateWhileLoading();
            }

            if (loadState == LoadRawAssetState.Loaded)
            {
                //加载结束
                CheckStateWhileLoaded();
            }
            
        }
        

        /// <inheritdoc />
        public override void OnPriorityChanged()
        {
            if (webRequestTask != null)
            {
                webRequestTask.Owner.ChangePriority(webRequestTask.MainTask,Group.Priority);
            }
        }

        /// <summary>
        /// 原生资源更新完毕回调
        /// </summary>
        private void OnRawAssetUpdated(BundleUpdateResult result)
        {
            if (!result.Success)
            {
                //下载失败
                loadState = LoadRawAssetState.Loaded;
                return;
            }

            //下载成功
            //Debug.Log($"下载成功：{result.UpdateInfo.Info}");
            loadState = LoadRawAssetState.Downloaded;
        }
        
        /// <summary>
        /// Web请求结束回调
        /// </summary>
        private void OnWebRequested(bool success, UnityWebRequest uwr)
        {
            loadState = LoadRawAssetState.Loaded;

            if (success)
            {
                byte[] rawAsset = uwr.downloadHandler.data;

                if (bundleRuntimeInfo.Manifest.EncryptOption == BundleEncryptOptions.XOr)
                {
                    //异或解密二进制
                    EncryptUtil.EncryptXOr(rawAsset);
                }
                
                assetRuntimeInfo.Asset = rawAsset;
                assetRuntimeInfo.MemorySize = (ulong)rawAsset.Length;

                CatAssetDatabase.SetAssetInstance(assetRuntimeInfo.Asset,assetRuntimeInfo);
            }
        }
        
        /// <summary>
        /// 调用加载完毕回调
        /// </summary>
        private void CallFinished(bool success)
        {
            if (!success)
            {
                handler.Error = "原生资源加载失败";
            }
            
            foreach (LoadRawAssetTask task in MergedTasks)
            {
                if (!task.IsCanceled)
                {
                    if (success)
                    {
                        //成功
                        assetRuntimeInfo.AddRefCount();
                        task.handler.SetAsset(assetRuntimeInfo.Asset);
                    }
                    else
                    {
                        //失败
                        task.handler.SetAsset(null);
                    }
                }
                else
                {
                    //已取消
                    task.handler.NotifyCanceled(task.CancelToken);
                }
            }
            
            if (success && IsAllCanceled)
            {
                //加载成功后所有任务都被取消了 这个资源没人要了 直接走卸载流程吧
                assetRuntimeInfo.AddRefCount();  //注意这里要先计数+1 才能正确执行后续的卸载流程
                CatAssetManager.UnloadAsset(assetRuntimeInfo.Asset);
            }
        }

        /// <summary>
        /// 创建原生资源加载任务的对象
        /// </summary>
        public static LoadRawAssetTask Create(TaskRunner owner, string name,AssetCategory category,AssetHandler handler,CancellationToken token)
        {
            LoadRawAssetTask task = ReferencePool.Get<LoadRawAssetTask>();
            task.CreateBase(owner,name,token);
            
            task.category = category;
            task.handler = handler;
            task.assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(name);
            task.bundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(task.assetRuntimeInfo.BundleManifest.BundleIdentifyName);
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            category = default;
            handler = default;

            assetRuntimeInfo = default;
            bundleRuntimeInfo = default;
            loadState = default;

            webRequestTask = default;
            
            startLoadTime = default;
        }
    }
}