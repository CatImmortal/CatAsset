using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    
    /// <summary>
    /// 原生资源加载任务
    /// </summary>
    public class LoadRawAssetTask<T> : BaseTask<LoadRawAssetTask<T>>
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
        private LoadAssetCallback<T> onFinished;

        private AssetRuntimeInfo assetRuntimeInfo;
        private BundleRuntimeInfo bundleRuntimeInfo;
        private LoadRawAssetState loadRawAssetState;
        private WebRequestCallback onWebRequestCallback;

        private bool needCancel;
        
        public LoadRawAssetTask()
        {
            onWebRequestCallback = OnWebRequest;
        }
        

        /// <inheritdoc />
        public override void Run()
        {
            if (assetRuntimeInfo.Asset != null)
            {
                //已加载好了
                loadRawAssetState = LoadRawAssetState.Loaded;
                return;
            }
            
            //未加载过
            WebRequestTask task = WebRequestTask.Create(Owner,bundleRuntimeInfo.LoadPath,bundleRuntimeInfo.LoadPath,null,onWebRequestCallback);
            Owner.AddTask(task,TaskPriority.Middle);
            loadRawAssetState = LoadRawAssetState.Loading;
            
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (loadRawAssetState)
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

        public override void Cancel()
        {
            needCancel = true;
        }

        /// <summary>
        /// Web请求结束回调
        /// </summary>
        private void OnWebRequest(bool success, UnityWebRequest uwr, object userdata)
        {
            loadRawAssetState = LoadRawAssetState.Loaded;

            if (success)
            {
                assetRuntimeInfo.Asset = uwr.downloadHandler.data;
                if (assetRuntimeInfo.AssetManifest.Length == default)
                {
                    assetRuntimeInfo.AssetManifest.Length = uwr.downloadHandler.data.Length;
                    bundleRuntimeInfo.Manifest.Length = uwr.downloadHandler.data.Length;
                }
                
                CatAssetDatabase.SetAssetInstance(assetRuntimeInfo.Asset,assetRuntimeInfo);
            }
            else
            {
                Debug.LogError($"原生资源:{bundleRuntimeInfo.LoadPath}加载失败");
            }
        }
        
        private void CheckStateWithLoading()
        {
            State = TaskState.Waiting;
        }
        
        private void CheckStateWithLoaded()
        {
            State = TaskState.Finished;

            if (assetRuntimeInfo.Asset != null)
            {
                //加载成功
                LoadAssetResult result = new LoadAssetResult(assetRuntimeInfo.Asset, category);
                T asset = result.GetAsset<T>();
                
                if (!needCancel)
                {
                    assetRuntimeInfo.AddRefCount();
                    
                    onFinished?.Invoke(true, asset, result);
                    
                    foreach (LoadRawAssetTask<T> task in MergedTasks)
                    {
                        if (!task.needCancel)
                        {
                            //增加已合并任务带来的引用计数
                            //保证1次成功的LoadRawAsset一定增加1个资源的引用计数
                            assetRuntimeInfo.AddRefCount();
                            task.onFinished?.Invoke(true, asset,result);
                        }
                   
                    }
                }
                else
                {
                    //被取消了
                    bool needUnload = true;
                    
                    //只是主任务被取消了 未取消的已合并任务还需要继续处理
                    foreach (LoadRawAssetTask<T> task in MergedTasks)
                    {
                        if (!task.needCancel)
                        {
                            needUnload = false;
                            assetRuntimeInfo.AddRefCount();  //增加已合并任务带来的引用计数
                            task.onFinished?.Invoke(true, asset,result);
                        }
                    }

                    if (needUnload)
                    {
                        //没有任何一个需要这个资源的已合并任务 直接卸载了
                        CatAssetManager.UnloadAsset(assetRuntimeInfo.Asset);
                    }
                }
            }
            else
            {
                //加载失败
                if (!needCancel)
                {
                    onFinished?.Invoke(false,default,default);
                }
                
                foreach (LoadRawAssetTask<T> task in MergedTasks)
                {
                    if (!task.needCancel)
                    {
                        task.onFinished?.Invoke(false,default,default);
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建原生资源加载任务的对象
        /// </summary>
        public static LoadRawAssetTask<T> Create(TaskRunner owner, string name,AssetCategory category,LoadAssetCallback<T> callback)
        {
            LoadRawAssetTask<T> task = ReferencePool.Get<LoadRawAssetTask<T>>();
            task.CreateBase(owner,name);
            
            task.category = category;
            task.onFinished = callback;
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
            onFinished = default;

            assetRuntimeInfo = default;
            bundleRuntimeInfo = default;
            loadRawAssetState = default;

            needCancel = default;
        }
    }
}