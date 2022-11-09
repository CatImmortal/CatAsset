using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包资源加载任务
    /// </summary>
    public class LoadBundledAssetTask : BaseTask
    {
        /// <summary>
        /// 资源包资源加载状态
        /// </summary>
        protected enum LoadBundledAssetState
        {
            None,

            /// <summary>
            /// 资源包未加载
            /// </summary>
            BundleNotLoad,
            
            /// <summary>
            /// 资源包加载中
            /// </summary>
            BundleLoading,

            /// <summary>
            /// 资源包加载结束
            /// </summary>
            BundleLoaded,

            /// <summary>
            /// 依赖资源未加载
            /// </summary>
            DependenciesNotLoad,
            
            /// <summary>
            /// 依赖资源加载中
            /// </summary>
            DependenciesLoading,

            /// <summary>
            /// 依赖资源加载结束
            /// </summary>
            DependenciesLoaded,

            /// <summary>
            /// 资源未加载
            /// </summary>
            AssetNotLoad,
            
            /// <summary>
            /// 资源加载中
            /// </summary>
            AssetLoading,

            /// <summary>
            /// 资源加载结束
            /// </summary>
            AssetLoaded,
        }

        private Type assetType;
        private AssetHandler handler;
        
        protected AssetRuntimeInfo AssetRuntimeInfo;
        protected BundleRuntimeInfo BundleRuntimeInfo;
        
        private readonly BundleLoadedCallback onBundleLoadedCallback;

        private int totalDependencyCount;
        private int loadFinishDependencyCount;
        private readonly List<AssetHandler> dependencyHandlers = new List<AssetHandler>();
        private readonly AssetLoadedCallback<Object> onDependencyLoadedCallback;


        protected LoadBundledAssetState LoadState;
        protected AsyncOperation Operation;

        protected bool NeedCancel;

        /// <inheritdoc />
        public override float Progress
        {
            get
            {
                if (Operation == null)
                {
                    return 0;
                }

                return Operation.progress;
            }
        }
        
        public LoadBundledAssetTask()
        {
            onBundleLoadedCallback = OnBundleLoaded;
            onDependencyLoadedCallback = OnDependencyLoaded;
        }
        

        /// <inheritdoc />
        public override void Run()
        {
            if (BundleRuntimeInfo.Bundle == null)
            {
                //资源包未加载
                LoadState = LoadBundledAssetState.BundleNotLoad;

            }
            else
            {
                //资源包已加载
                //但资源未加载或已加载未使用 加载一遍依赖
                LoadState = LoadBundledAssetState.DependenciesNotLoad;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (LoadState == LoadBundledAssetState.BundleNotLoad)
            {
                //资源包未加载
                CheckStateWithBundleNotLoad();
            }

            if (LoadState == LoadBundledAssetState.BundleLoading)
            {
                //资源包加载中
                CheckStateWithBundleLoading();
            }
            
            if (LoadState == LoadBundledAssetState.BundleLoaded)
            {
                //资源包加载结束
                CheckStateWithBundleLoaded();
            }
            
            if (LoadState == LoadBundledAssetState.DependenciesNotLoad)
            {
                //依赖资源未加载
                CheckStateWithDependenciesNotLoad();
            }
            
            if (LoadState == LoadBundledAssetState.DependenciesLoading)
            {
                //依赖资源加载中
                CheckStateWithDependenciesLoading();
            }
            
            if (LoadState == LoadBundledAssetState.DependenciesLoaded)
            {
                //依赖资源加载结束
                CheckStateWithDependenciesLoaded();
            }
            
            if (LoadState == LoadBundledAssetState.AssetNotLoad)
            {
                //资源未加载
                CheckStateWithAssetNotLoad();
            }
            
            if (LoadState == LoadBundledAssetState.AssetLoading)
            {
                //资源加载中
                CheckStateWithAssetLoading();
            }
            
            if (LoadState == LoadBundledAssetState.AssetLoaded)
            {
                //资源加载结束
                CheckStateWithAssetLoaded();
            }
        }

        /// <inheritdoc />
        public override void Cancel()
        {
            NeedCancel = true;
        }

        /// <summary>
        /// 资源包加载结束的回调
        /// </summary>
        private void OnBundleLoaded(bool success)
        {
            LoadState = LoadBundledAssetState.BundleLoaded;
        }

        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(AssetHandler<Object> handler)
        {
            loadFinishDependencyCount++;
        }

        
        private void CheckStateWithBundleNotLoad()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundledAssetState.BundleLoading;
            
            BaseTask task;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                //WebGL平台特殊处理下
                task = LoadWebBundleTask.Create(Owner, BundleRuntimeInfo.Manifest.RelativePath,
                    onBundleLoadedCallback);
            }
            else
            {
                task = LoadBundleTask.Create(Owner, BundleRuntimeInfo.Manifest.RelativePath,
                    onBundleLoadedCallback);
            }
            Owner.AddTask(task, TaskPriority.Middle);
        }
        
        private void CheckStateWithBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWithBundleLoaded()
        {
            if (BundleRuntimeInfo.Bundle == null)
            {
                //资源包加载失败
                State = TaskState.Finished;
                
                CallFinished(false);

            }
            else
            {
                //资源包加载成功
                State = TaskState.Waiting;
                LoadState = LoadBundledAssetState.DependenciesNotLoad;
            }
        }

        private void CheckStateWithDependenciesNotLoad()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundledAssetState.DependenciesLoading;
            
            //加载依赖
            totalDependencyCount = AssetRuntimeInfo.AssetManifest.Dependencies.Count;
            foreach (string dependency in AssetRuntimeInfo.AssetManifest.Dependencies)
            {
                AssetHandler<Object> dependencyHandler = CatAssetManager.LoadAssetAsync<Object>(dependency,default,TaskPriority.Middle);
                dependencyHandler.OnLoaded += onDependencyLoadedCallback;
                dependencyHandlers.Add(dependencyHandler);
            }
        }
        
        private void CheckStateWithDependenciesLoading()
        {
            State = TaskState.Waiting;
            
            if (loadFinishDependencyCount != totalDependencyCount)
            {
                //依赖还未加载完
                return;
            }

            LoadState = LoadBundledAssetState.DependenciesLoaded;
        }

        private void CheckStateWithDependenciesLoaded()
        {
            State = TaskState.Waiting;

            if (AssetRuntimeInfo.BundleManifest.IsScene ||  AssetRuntimeInfo.Asset == null)
            {
                //是场景 或未加载过 需要加载
                LoadState = LoadBundledAssetState.AssetNotLoad;
            }
            else
            {
                //已加载过
                LoadState = LoadBundledAssetState.AssetLoaded;
            }
        }

        private void CheckStateWithAssetNotLoad()
        {
            State = TaskState.Running;
            LoadState = LoadBundledAssetState.AssetLoading;
            
            LoadAsync();
        }
        
        private void CheckStateWithAssetLoading()
        {
            State = TaskState.Running;

            if (Operation != null && Operation.isDone)
            {
                LoadState = LoadBundledAssetState.AssetLoaded;
                LoadDone();
            }
        }

        private void CheckStateWithAssetLoaded()
        {
            State = TaskState.Finished;

            
            if (IsLoadFailed())
            {
                //资源加载失败
                //将已加载好的依赖都卸载一遍
                foreach (AssetHandler dependencyHandler in dependencyHandlers)
                {
                    dependencyHandler.Dispose();
                }
                dependencyHandlers.Clear();

                //检查下资源包的生命周期 可能需要卸载了
                BundleRuntimeInfo.CheckLifeCycle();
                
                CallFinished(false);
            }
            else
            {
                //资源加载成功 或 是已加载好的

                //添加依赖链记录
                foreach (AssetHandler dependencyHandler in dependencyHandlers)
                {
                    if (dependencyHandler.State == HandlerState.Success)
                    {
                        AssetRuntimeInfo depInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependencyHandler.AssetObj);
                        
                        //将自身设置为依赖资源的下游资源
                        depInfo.AddDownStream(AssetRuntimeInfo);
                        
                        //如果依赖了其他资源包里的资源 还需要设置 自身所在资源包 与 依赖所在资源包 的上下游关系
                        if (!depInfo.BundleManifest.Equals(AssetRuntimeInfo.BundleManifest))
                        {
                            BundleRuntimeInfo depBundleInfo =
                                CatAssetDatabase.GetBundleRuntimeInfo(depInfo.BundleManifest.RelativePath);
                        
                            depBundleInfo.AddDownStream(BundleRuntimeInfo);
                            BundleRuntimeInfo.AddUpStream(depBundleInfo);
                        }
                    }
                }

                CallFinished(true);
            }
            
            
        }

        
        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            Operation = BundleRuntimeInfo.Bundle.LoadAssetAsync(Name,assetType);
        }

        /// <summary>
        /// 异步加载结束
        /// </summary>
        protected virtual void LoadDone()
        {
            AssetBundleRequest request = (AssetBundleRequest) Operation;
            AssetRuntimeInfo.Asset = request.asset;

            if (AssetRuntimeInfo.Asset != null)
            {
                //添加关联
                CatAssetDatabase.SetAssetInstance(AssetRuntimeInfo.Asset, AssetRuntimeInfo);
            }
        }
        
        /// <summary>
        /// 资源是否加载失败
        /// </summary>
        protected virtual bool IsLoadFailed()
        {
            return AssetRuntimeInfo.Asset == null;
        }

        /// <summary>
        /// 调用加载完毕回调
        /// </summary>
        protected virtual void CallFinished(bool success)
        {
            if (!success)
            {
                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                
                //加载失败 通知所有未取消的加载任务
                if (!NeedCancel)
                {
                    handler.SetAsset(null);
                }
                
                foreach (LoadBundledAssetTask task in MergedTasks)
                {
                    if (!task.NeedCancel)
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
                    AssetRuntimeInfo.AddRefCount();  //注意这里要先计数+1 才能正确执行后续的卸载流程
                    CatAssetManager.UnloadAsset(AssetRuntimeInfo.Asset);
                    return;
                }

                //加载成功 通知所有未取消的加载任务
                if (!NeedCancel)
                {
                    AssetRuntimeInfo.AddRefCount();
                    handler.SetAsset(AssetRuntimeInfo.Asset);
                }
                foreach (LoadBundledAssetTask task in MergedTasks)
                {
                    if (!task.NeedCancel)
                    {
                        AssetRuntimeInfo.AddRefCount();
                        task.handler.SetAsset(AssetRuntimeInfo.Asset);
                    }
                   
                }
            }
        }
        

        /// <summary>
        /// 是否全部加载任务都被取消了
        /// </summary>
        private bool IsAllCancel()
        {
            foreach (LoadBundledAssetTask task in MergedTasks)
            {
                if (!task.NeedCancel)
                {
                    return false;
                }
            }

            return NeedCancel;
        }

        /// <summary>
        /// 创建资源加载任务的对象
        /// </summary>
        public static LoadBundledAssetTask Create(TaskRunner owner, string name,Type assetType,AssetHandler handler)
        {
            LoadBundledAssetTask task = ReferencePool.Get<LoadBundledAssetTask>();
            task.CreateBase(owner, name);

            task.assetType = assetType;
            task.handler = handler;
            
            task.AssetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.RelativePath);
            

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            assetType = default;
            handler = default;
            
            AssetRuntimeInfo = default;
            BundleRuntimeInfo = default;
            
            totalDependencyCount = default;
            loadFinishDependencyCount = default;
            
            //释放掉所有依赖资源的句柄
            foreach (AssetHandler dependencyHandler in dependencyHandlers)
            {
                dependencyHandler.Release();
            }
            dependencyHandlers.Clear();
            
            LoadState = default;
            Operation = default;
            NeedCancel = default;
        }
    }
}