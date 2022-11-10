using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    
    /// <summary>
    /// 原生资源加载任务
    /// </summary>
    public class LoadRawAssetTask : BaseTask
    {
        /// <summary>
        /// 原生资源加载状态
        /// </summary>
        private enum LoadRawAssetState
        {
            None = 0,
            
            /// <summary>
            /// 资源加载中
            /// </summary>
            Loading = 1,

            /// <summary>
            /// 资源加载结束
            /// </summary>
            Loaded = 2,
        }

        private AssetCategory category;
        private AssetHandler handler;

        private AssetRuntimeInfo assetRuntimeInfo;
        private BundleRuntimeInfo bundleRuntimeInfo;
        private LoadRawAssetState loadState;
        private readonly WebRequestedCallback onWebRequestedCallback;

        /// <summary>
        /// 是否被取消，handler为空 或者 handler被token取消 就认为此任务被取消了
        /// </summary>
        private bool IsCanceled => handler == null || handler.IsTokenCanceled;
        
        public LoadRawAssetTask()
        {
            onWebRequestedCallback = OnWebRequested;
        }
        

        /// <inheritdoc />
        public override void Run()
        {
            if (assetRuntimeInfo.Asset != null)
            {
                //虽然引用计数为0 但是已加载好了
                loadState = LoadRawAssetState.Loaded;
                return;
            }
            
            //未加载过
            WebRequestTask task = WebRequestTask.Create(Owner,bundleRuntimeInfo.LoadPath,bundleRuntimeInfo.LoadPath,null,onWebRequestedCallback);
            Owner.AddTask(task,TaskPriority.Low);
            loadState = LoadRawAssetState.Loading;
            
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (loadState)
            {

                case LoadRawAssetState.Loading:
                    //加载中
                    CheckStateWithLoading();
                    break;
                
                case LoadRawAssetState.Loaded:
                    //加载结束
                    CheckStateWithLoaded();
                    break;

            }

        }

        /// <inheritdoc />
        public override void Cancel()
        {
            handler = null;
        }

        /// <summary>
        /// Web请求结束回调
        /// </summary>
        private void OnWebRequested(bool success, UnityWebRequest uwr, object userdata)
        {
            loadState = LoadRawAssetState.Loaded;

            if (success)
            {
                byte[] rawAsset = uwr.downloadHandler.data;
                assetRuntimeInfo.Asset = rawAsset;
                if (assetRuntimeInfo.AssetManifest.Length == default)
                {
                    assetRuntimeInfo.AssetManifest.Length = rawAsset.Length;
                    bundleRuntimeInfo.Manifest.Length = rawAsset.Length;
                }
                
                CatAssetDatabase.SetAssetInstance(assetRuntimeInfo.Asset,assetRuntimeInfo);
            }
        }
        
        private void CheckStateWithLoading()
        {
            State = TaskState.Waiting;
        }
        
        private void CheckStateWithLoaded()
        {
            State = TaskState.Finished;

            if (assetRuntimeInfo == null)
            {
                Debug.LogError($"原生资源加载失败:{bundleRuntimeInfo.LoadPath}");
                
                //资源加载失败
                if (!IsCanceled)
                {
                    handler.SetAsset(null);
                }
                
                foreach (LoadRawAssetTask task in MergedTasks)
                {
                    if (!task.IsCanceled)
                    {
                        task.handler.SetAsset(null);
                    }
                }
            }
            else
            {
                if (IsAllCancel())
                {
                    //所有任务都被取消了 这个资源没人要了 直接卸载吧
                    assetRuntimeInfo.AddRefCount();  //注意这里要先计数+1 才能正确执行后续的卸载流程
                    CatAssetManager.UnloadAsset(assetRuntimeInfo.Asset);
                    return;
                }

                //加载成功 通知所有未取消的加载任务
                if (!IsCanceled)
                {
                    assetRuntimeInfo.AddRefCount();
                    handler.SetAsset(assetRuntimeInfo.Asset);
                }
                foreach (LoadRawAssetTask task in MergedTasks)
                {
                    if (!task.IsCanceled)
                    {
                        assetRuntimeInfo.AddRefCount();
                        task.handler.SetAsset(assetRuntimeInfo.Asset);
                    }
                }
            }
        }
        
        /// <summary>
        /// 是否全部加载任务都被取消了
        /// </summary>
        private bool IsAllCancel()
        {
            foreach (LoadRawAssetTask task in MergedTasks)
            {
                if (!task.IsCanceled)
                {
                    return false;
                }
            }

            return IsCanceled;
        }
        
        /// <summary>
        /// 创建原生资源加载任务的对象
        /// </summary>
        public static LoadRawAssetTask Create(TaskRunner owner, string name,AssetCategory category,AssetHandler handler)
        {
            LoadRawAssetTask task = ReferencePool.Get<LoadRawAssetTask>();
            task.CreateBase(owner,name);
            
            task.category = category;
            task.handler = handler;
            task.assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(name);
            task.bundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(task.assetRuntimeInfo.BundleManifest.RelativePath);
            
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
        }
    }
}