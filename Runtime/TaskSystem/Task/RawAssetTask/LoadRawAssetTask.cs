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
        private object userdata;
        private InternalLoadAssetCallback onFinished;

        private AssetRuntimeInfo assetRuntimeInfo;
        private BundleRuntimeInfo bundleRuntimeInfo;
        private LoadRawAssetState loadState;
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
                //虽然引用计数为0 但是已加载好了
                loadState = LoadRawAssetState.Loaded;
                return;
            }
            
            //未加载过
            WebRequestTask task = WebRequestTask.Create(Owner,bundleRuntimeInfo.LoadPath,bundleRuntimeInfo.LoadPath,null,onWebRequestCallback);
            Owner.AddTask(task,TaskPriority.Middle);
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

        public override void Cancel()
        {
            needCancel = true;
        }

        /// <summary>
        /// Web请求结束回调
        /// </summary>
        private void OnWebRequest(bool success, UnityWebRequest uwr, object userdata)
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

            if (assetRuntimeInfo == null)
            {
                //资源加载失败
                if (!needCancel)
                {
                    onFinished?.Invoke(userdata,default);
                }
                
                foreach (LoadRawAssetTask task in MergedTasks)
                {
                    if (!task.needCancel)
                    {
                        task.onFinished?.Invoke(task.userdata,default);
                    }
                }
            }
            else
            {
                if (IsAllCancel())
                {
                    //所有任务都被取消了 这个资源没人要了 直接卸载吧
                    assetRuntimeInfo.AddRefCount();  //注意这里要先计数+1 才能正确执行后续的卸载流程
                    CatAssetManager.InternalUnloadAsset(assetRuntimeInfo);
                    return;
                }
                
                LoadAssetResult result = new LoadAssetResult(assetRuntimeInfo.Asset, category);

                //加载成功 通知所有未取消的加载任务
                if (!needCancel)
                {
                    assetRuntimeInfo.AddRefCount();
                    onFinished?.Invoke(userdata,result);
                }
                foreach (LoadRawAssetTask task in MergedTasks)
                {
                    if (!task.needCancel)
                    {
                        assetRuntimeInfo.AddRefCount();
                        task.onFinished?.Invoke(task.userdata,result);
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
                if (!task.needCancel)
                {
                    return false;
                }
            }

            return needCancel;
        }
        
        /// <summary>
        /// 创建原生资源加载任务的对象
        /// </summary>
        public static LoadRawAssetTask Create(TaskRunner owner, string name,AssetCategory category,object userdata,InternalLoadAssetCallback callback)
        {
            LoadRawAssetTask task = ReferencePool.Get<LoadRawAssetTask>();
            task.CreateBase(owner,name);
            
            task.category = category;
            task.userdata = userdata;
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
            userdata = null;
            onFinished = default;

            assetRuntimeInfo = default;
            bundleRuntimeInfo = default;
            loadState = default;

            needCancel = default;
        }
    }
}