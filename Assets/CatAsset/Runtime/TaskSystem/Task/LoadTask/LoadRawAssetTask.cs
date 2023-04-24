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

        private WebRequestTask webRequestTask;
        private readonly WebRequestedCallback onWebRequestedCallback;

        private float startLoadTime;
        
        
        public LoadRawAssetTask()
        {
            onWebRequestedCallback = OnWebRequested;
        }
        

        /// <inheritdoc />
        public override void Run()
        {
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

        /// <inheritdoc />
        public override void Update()
        {
            CheckAllCanceled();
            
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
                        assetRuntimeInfo.AddRefCount();
                        task.handler.SetAsset(assetRuntimeInfo.Asset);
                    }
                    else
                    {
                        task.handler.SetAsset(null);
                    }
                }
                else
                {
                    //已取消
                    task.handler.NotifyCanceled(CancelToken);
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