using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{

    /// <summary>
    /// 原生资源加载任务完成回调的原型
    /// </summary>
    public delegate void LoadRawAssetCallback(bool success, byte[] asset, object userdata);
    
    /// <summary>
    /// 原生资源加载任务
    /// </summary>
    public class LoadRawAssetTask : BaseTask<LoadRawAssetTask>
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
        
        private object userdata;
        private LoadRawAssetCallback onFinished;

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
            assetRuntimeInfo.AddRefCount();
            loadRawAssetState = LoadRawAssetState.Loading;
            
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (loadRawAssetState)
            {

                case LoadRawAssetState.Loading:
                    CheckStateWithLoading();
                    break;
                
                case LoadRawAssetState.Loaded:
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
                CatAssetManager.SetAssetInstance(assetRuntimeInfo.Asset,assetRuntimeInfo);
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
                if (!needCancel)
                {
                    onFinished?.Invoke(true, (byte[]) assetRuntimeInfo.Asset, userdata);
                    foreach (LoadRawAssetTask task in MergedTasks)
                    {
                        if (!task.needCancel)
                        {
                            //增加已合并任务带来的引用计数
                            //保证1次成功的LoadRawAsset一定增加1个资源的引用计数
                            assetRuntimeInfo.AddRefCount();
                            task.onFinished?.Invoke(true, (byte[]) assetRuntimeInfo.Asset, task.userdata);
                        }
                   
                    }
                }
                else
                {
                    //被取消了
                    bool needUnload = true;
                   
                    
                    //只是主任务被取消了 未取消的已合并任务还需要继续处理
                    foreach (LoadRawAssetTask task in MergedTasks)
                    {
                        if (!task.needCancel)
                        {
                            needUnload = false;
                            assetRuntimeInfo.AddRefCount();  //增加已合并任务带来的引用计数
                            task.onFinished?.Invoke(true, (byte[]) assetRuntimeInfo.Asset, task.userdata);
                        }
                    }

                    if (!needUnload)
                    {
                        //至少有一个需要这个资源的已合并任务 那就只需要将主任务增加的那1个引用计数减去就行
                        assetRuntimeInfo.SubRefCount();
                    }
                    else
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
                    onFinished?.Invoke(false,null,userdata);
                }
                
                foreach (LoadRawAssetTask task in MergedTasks)
                {
                    if (!task.needCancel)
                    {
                        task.onFinished?.Invoke(false,null,task.userdata);
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建原生资源加载任务的对象
        /// </summary>
        public static LoadRawAssetTask Create(TaskRunner owner, string name,object userdata,LoadRawAssetCallback callback)
        {
            LoadRawAssetTask task = ReferencePool.Get<LoadRawAssetTask>();
            task.CreateBase(owner,name);
            
            task.userdata = userdata;
            task.onFinished = callback;
            task.assetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(name);
            task.bundleRuntimeInfo =
                CatAssetManager.GetBundleRuntimeInfo(task.assetRuntimeInfo.BundleManifest.RelativePath);
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            userdata = default;
            onFinished = default;

            assetRuntimeInfo = default;
            bundleRuntimeInfo = default;
            loadRawAssetState = default;

            needCancel = default;
        }
    }
}