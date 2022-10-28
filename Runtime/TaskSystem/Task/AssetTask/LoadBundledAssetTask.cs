using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载任务完成回调的原型
    /// </summary>
    public delegate void LoadAssetCallback<in T>(T asset, LoadAssetResult result);

    /// <summary>
    /// 内部的资源加载任务完成回调的原型
    /// </summary>
    public delegate void InternalLoadAssetCallback(object userdata, LoadAssetResult result);
    
    /// <summary>
    /// 资源包资源加载任务
    /// </summary>
    public class LoadBundledAssetTask : BaseTask
    {
        /// <summary>
        /// 资源包资源加载状态
        /// </summary>
        protected enum LoadBundleAssetState
        {
            None = 0,

            /// <summary>
            /// 资源包加载中
            /// </summary>
            BundleLoading = 1,

            /// <summary>
            /// 资源包加载结束
            /// </summary>
            BundleLoaded = 2,

            /// <summary>
            /// 依赖资源加载中
            /// </summary>
            DependenciesLoading = 3,

            /// <summary>
            /// 依赖资源加载结束
            /// </summary>
            DependenciesLoaded = 4,

            /// <summary>
            /// 资源加载中
            /// </summary>
            AssetLoading = 5,

            /// <summary>
            /// 资源加载结束
            /// </summary>
            AssetLoaded = 6,
        }

        private Type assetType;
        private object userdata;
        private InternalLoadAssetCallback onFinished;
        
        protected AssetRuntimeInfo AssetRuntimeInfo;
        protected BundleRuntimeInfo BundleRuntimeInfo;
        
        private LoadBundleCallback onBundleLoadedCallback;

        private int totalDependencyCount;
        private int loadFinishDependencyCount;
        private LoadAssetCallback<Object> onDependencyLoadedCallback;


        protected LoadBundleAssetState LoadState;
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
                
                LoadState = LoadBundleAssetState.BundleLoading;

            }
            else
            {
                //资源包已加载
                //但资源未加载或已加载未使用
                LoadState = LoadBundleAssetState.BundleLoaded;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (LoadState)
            {
                case LoadBundleAssetState.BundleLoading:
                    //1.资源包加载中
                    CheckStateWithBundleLoading();
                    break;
                
                case LoadBundleAssetState.BundleLoaded:
                    //2.资源包加载结束，开始加载依赖资源
                    CheckStateWithBundleLoaded();
                    break;
                
                case LoadBundleAssetState.DependenciesLoading:
                    //3.依赖资源加载中
                    CheckStateWithDependenciesLoading();
                    break;
                
                case LoadBundleAssetState.DependenciesLoaded:
                    //4.依赖资源加载结束，开始加载主资源
                    CheckStateWithDependenciesLoaded();
                    break;
                
                case LoadBundleAssetState.AssetLoading:
                    //5.检查主资源是否加载结束
                    CheckStateWithAssetLoading();
                    break;
                
                case LoadBundleAssetState.AssetLoaded:
                    //6.主资源加载结束，检查是否加载成功
                    CheckStateWithAssetLoaded();
                    break;
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
            if (!success)
            {
                //资源包加载失败
                LoadState = LoadBundleAssetState.None;
                State = TaskState.Finished;

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                CallFinished(false);

            }
            else
            {
                //资源包加载成功
                LoadState = LoadBundleAssetState.BundleLoaded;
            }
        }

        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(Object asset,LoadAssetResult result)
        {
            loadFinishDependencyCount++;
        }


        #region 资源加载状态检查

        private void CheckStateWithBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWithBundleLoaded()
        {
            State = TaskState.Waiting;

            //加载依赖
            LoadState = LoadBundleAssetState.DependenciesLoading;
            totalDependencyCount = AssetRuntimeInfo.AssetManifest.Dependencies.Count;
            foreach (string dependency in AssetRuntimeInfo.AssetManifest.Dependencies)
            {
                CatAssetManager.LoadAssetAsync(dependency, onDependencyLoadedCallback,TaskPriority.Middle);
            }
        }

        private void CheckStateWithDependenciesLoading()
        {
            State = TaskState.Waiting;
            
            if (loadFinishDependencyCount != totalDependencyCount)
            {
                return;
            }

            LoadState = LoadBundleAssetState.DependenciesLoaded;
        }

        private void CheckStateWithDependenciesLoaded()
        {
            State = TaskState.Running;
            
            if (AssetRuntimeInfo.BundleManifest.IsScene ||  AssetRuntimeInfo.Asset == null)
            {
                //是场景资源 或者 是未加载过的普通资源 发起异步加载
                //场景资源每次加载都相当于重新实例化了，无法复用
                LoadAsync();
                LoadState = LoadBundleAssetState.AssetLoading;
            }
            else
            {
                //已加载过 直接转移到AssetLoaded状态
                LoadState = LoadBundleAssetState.AssetLoaded;
            }
        }

        private void CheckStateWithAssetLoading()
        {
            if (Operation != null && !Operation.isDone)
            {
                State = TaskState.Running;
                return;
            }

            LoadState = LoadBundleAssetState.AssetLoaded;
            LoadDone();
        }

        private void CheckStateWithAssetLoaded()
        {
            State = TaskState.Finished;

            
            if (IsLoadFailed())
            {
                //资源加载失败
                
                //将已加载好的依赖都卸载一遍
                foreach (string dependencyName in AssetRuntimeInfo.AssetManifest.Dependencies)
                {
                    AssetRuntimeInfo dependencyInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependencyName);
                    if (dependencyInfo.Asset != null)
                    {
                        CatAssetManager.InternalUnloadAsset(dependencyInfo);
                    }
                }
                
                //检查下资源包的生命周期 可能需要卸载了
                BundleRuntimeInfo.CheckLifeCycle();
                
                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                
                CallFinished(false);
            }
            else
            {
                //资源加载成功 或 是已加载好的

                //添加依赖链记录
                foreach (string dependencyName in AssetRuntimeInfo.AssetManifest.Dependencies)
                {
                    AssetRuntimeInfo depAssetInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependencyName);
                    if (depAssetInfo.Asset != null)
                    {
                        depAssetInfo.AddDownStream(AssetRuntimeInfo);
                    }
                    
                    if (!depAssetInfo.BundleManifest.Equals(AssetRuntimeInfo.BundleManifest))
                    {
                        BundleRuntimeInfo depBundleInfo =
                            CatAssetDatabase.GetBundleRuntimeInfo(depAssetInfo.BundleManifest.RelativePath);
                        
                        depBundleInfo.AddDownStream(BundleRuntimeInfo);
                        BundleRuntimeInfo.AddUpStream(depBundleInfo);
                    }
                }
                
                CallFinished(true);
            }
            
            
        }
        
        #endregion

        
        #region 给子类重写的虚方法

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
                //加载失败 通知所有未取消的加载任务
                if (!NeedCancel)
                {
                    onFinished?.Invoke(userdata,default);
                }
                
                foreach (LoadBundledAssetTask task in MergedTasks)
                {
                    if (!task.NeedCancel)
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
                    AssetRuntimeInfo.AddRefCount();  //注意这里要先计数+1 才能正确执行后续的卸载流程
                    CatAssetManager.InternalUnloadAsset(AssetRuntimeInfo);
                    return;
                }
                
                LoadAssetResult result = new LoadAssetResult(AssetRuntimeInfo.Asset, AssetCategory.InternalBundledAsset);

                //加载成功 通知所有未取消的加载任务
                if (!NeedCancel)
                {
                    AssetRuntimeInfo.AddRefCount();
                    onFinished?.Invoke(userdata,result);
                }
                foreach (LoadBundledAssetTask task in MergedTasks)
                {
                    if (!task.NeedCancel)
                    {
                        AssetRuntimeInfo.AddRefCount();
                        task.onFinished?.Invoke(task.userdata,result);
                    }
                   
                }
            }
        }
        

        /// <summary>
        /// 是否全部加载任务都被取消了
        /// </summary>
        protected bool IsAllCancel()
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
        
        #endregion

        /// <summary>
        /// 创建资源加载任务的对象
        /// </summary>
        public static LoadBundledAssetTask Create(TaskRunner owner, string name,Type assetType,object userdata,
            InternalLoadAssetCallback callback)
        {
            LoadBundledAssetTask task = ReferencePool.Get<LoadBundledAssetTask>();
            task.CreateBase(owner, name);

            task.assetType = assetType;
            task.userdata = userdata;
            task.onFinished = callback;
            
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
            userdata = default;
            onFinished = default;
            AssetRuntimeInfo = default;
            BundleRuntimeInfo = default;
            totalDependencyCount = default;
            loadFinishDependencyCount = default;
            LoadState = default;
            Operation = default;
            NeedCancel = default;
        }
    }
}