using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载任务完成回调的原型
    /// </summary>
    public delegate void LoadAssetCallback<in T>(bool success, T asset,LoadAssetResult result);

    /// <summary>
    /// 资源包资源加载任务
    /// </summary>
    public class LoadBundledAssetTask<T> : BaseTask<LoadBundledAssetTask<T>>
    {
        /// <summary>
        /// 资源包资源加载状态
        /// </summary>
        private enum LoadBundleAssetState
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
        
        private LoadAssetCallback<T> onFinished;
        
        protected AssetRuntimeInfo AssetRuntimeInfo;
        protected BundleRuntimeInfo BundleRuntimeInfo;
        
        private LoadBundleCallback onBundleLoadedCallback;

        private int totalDependencyCount;
        private int loadFinishDependencyCount;
        private LoadAssetCallback<Object> onDependencyLoadedCallback;


        private LoadBundleAssetState loadBundleAssetState;
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
            if (AssetRuntimeInfo.UseCount > 0)
            {
                //资源已加载好 且正在使用中
                //无需考虑操作依赖资源的引用计数与引用记录
                //直接增加自身引用计数然后转移到DependenciesLoaded状态即可
                AssetRuntimeInfo.AddUseCount();
                loadBundleAssetState = LoadBundleAssetState.DependenciesLoaded;
                return;
            }
            
            if (BundleRuntimeInfo.Bundle == null)
            {
                //资源包未加载
                LoadBundleTask task = LoadBundleTask.Create(Owner, BundleRuntimeInfo.Manifest.RelativePath, null,
                    onBundleLoadedCallback);
                Owner.AddTask(task, TaskPriority.Height);
                
                loadBundleAssetState = LoadBundleAssetState.BundleLoading;
                return;
            }
            
            //资源包已加载
            //但资源未加载或未使用
            loadBundleAssetState = LoadBundleAssetState.BundleLoaded;
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (loadBundleAssetState)
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
        private void OnBundleLoaded(bool success, object userdata)
        {
            if (!success)
            {
                //资源包加载失败
                loadBundleAssetState = LoadBundleAssetState.None;
                State = TaskState.Finished;

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                CallFinished(false);

            }
            else
            {
                //资源包加载成功
                loadBundleAssetState = LoadBundleAssetState.BundleLoaded;
            }
        }

        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(bool success, Object asset,LoadAssetResult result)
        {
            loadFinishDependencyCount++;

            if (success)
            {
                AssetRuntimeInfo dependencyAssetInfo = CatAssetDatabase.GetAssetRuntimeInfo(asset);
                BundleRuntimeInfo dependencyBundleInfo =
                    CatAssetDatabase.GetBundleRuntimeInfo(dependencyAssetInfo.BundleManifest.RelativePath);

                //添加到依赖资源的上游
                dependencyAssetInfo.AddUpStream(AssetRuntimeInfo);
                
                if (!dependencyBundleInfo.Equals(BundleRuntimeInfo))
                {
                    //依赖了其他资源包的资源 需要记录此资源所属资源包和所依赖的其他资源包的上下游关系
                    dependencyBundleInfo.AddUpStream(BundleRuntimeInfo);
                    BundleRuntimeInfo.AddDownStream(dependencyBundleInfo);
                }
            }
        }

        
        #region 资源加载状态检查

        private void CheckStateWithBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWithBundleLoaded()
        {
            State = TaskState.Waiting;
            
            //这里要在确认资源包已加载后，就马上增加资源的引用计数和使用记录
            //防止在依赖资源加载完成前 其他地方意外的触发了资源包的卸载
            AssetRuntimeInfo.AddUseCount();

            if (AssetRuntimeInfo.AssetManifest.Dependencies == null)
            {
                //没有依赖需要加载
                loadBundleAssetState = LoadBundleAssetState.DependenciesLoaded;
                totalDependencyCount = 0;
            }
            else
            {
                //加载依赖
                loadBundleAssetState = LoadBundleAssetState.DependenciesLoading;
                totalDependencyCount = AssetRuntimeInfo.AssetManifest.Dependencies.Count;
                foreach (string dependency in AssetRuntimeInfo.AssetManifest.Dependencies)
                {
                    CatAssetManager.InternalLoadAsset(dependency,onDependencyLoadedCallback);
                }
            }
        }

        private void CheckStateWithDependenciesLoading()
        {
            State = TaskState.Waiting;
            
            if (loadFinishDependencyCount != totalDependencyCount)
            {
                return;
            }

            loadBundleAssetState = LoadBundleAssetState.DependenciesLoaded;
        }

        private void CheckStateWithDependenciesLoaded()
        {
            State = TaskState.Running;
            
            if (AssetRuntimeInfo.Asset == null)
            {
                //未加载过 发起异步加载
                //只对普通资源这样复用，场景资源的AssetRuntimeInfo.Asset一定是空的
                //因为场景资源每次加载都相当于重新Instantiate了，没有复用的概念
                LoadAsync();
                loadBundleAssetState = LoadBundleAssetState.AssetLoading;
            }
            else
            {
                //已加载过 直接转移到AssetLoaded状态
                loadBundleAssetState = LoadBundleAssetState.AssetLoaded;
            }
        }

        private void CheckStateWithAssetLoading()
        {
            if (!Operation.isDone)
            {
                State = TaskState.Running;
                return;
            }

            loadBundleAssetState = LoadBundleAssetState.AssetLoaded;
            LoadDone();
        }

        private void CheckStateWithAssetLoaded()
        {
            State = TaskState.Finished;

            if (IsLoadFailed())
            {
                //资源加载失败
                
                //减少引用计数
                AssetRuntimeInfo.SubUseCount();
                
                //卸载已加载好的依赖 清除上游记录
                if (totalDependencyCount > 0)
                {
                    foreach (string dependencyName in AssetRuntimeInfo.AssetManifest.Dependencies)
                    {
                        AssetRuntimeInfo dependencyInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependencyName);
                        if (dependencyInfo.Asset != null)
                        {
                            //将已加载的依赖都卸载一遍
                            dependencyInfo.RemoveUpStream(AssetRuntimeInfo);
                            CatAssetManager.UnloadAsset(dependencyInfo.Asset);
                        }
                    }
                }
                

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                CallFinished(false);
                
                return;
            }
            
            CallFinished(true);
        }
        
        #endregion

        
        #region 给子类重写的虚方法

        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            Operation = BundleRuntimeInfo.Bundle.LoadAssetAsync(Name, typeof(T));
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
            if (success)
            {
                LoadAssetResult result = new LoadAssetResult(AssetRuntimeInfo.Asset, AssetCategory.InternalBundledAsset);
                T asset = result.GetAsset<T>();
                
                if (!NeedCancel)
                {
                    onFinished?.Invoke(true, asset,result);
                    foreach (LoadBundledAssetTask<T> task in MergedTasks)
                    {
                        if (!task.NeedCancel)
                        {
                            //增加已合并任务带来的引用计数
                            //保证1次成功的LoadAsset一定增加1个资源的引用计数
                            AssetRuntimeInfo.AddUseCount();
                            task.onFinished?.Invoke(true, asset,result);
                        }
                   
                    }
                }
                else
                {
                    //被取消了
                    
                    bool needUnload = true;

                    //只是主任务被取消了 未取消的已合并任务还需要继续处理
                    foreach (LoadBundledAssetTask<T> task in MergedTasks)
                    {
                        if (!task.NeedCancel)
                        {
                            needUnload = false;
                            AssetRuntimeInfo.AddUseCount();  //增加已合并任务带来的引用计数
                            task.onFinished?.Invoke(true, asset,result);
                        }
                    }

                    if (!needUnload)
                    {
                       //至少有一个需要这个资源的已合并任务 那就只需要将主任务增加的那1个引用计数减去就行
                       AssetRuntimeInfo.SubUseCount();
                    }
                    else
                    {
                        //没有任何一个需要这个资源的已合并任务 直接卸载了
                        CatAssetManager.UnloadAsset(AssetRuntimeInfo.Asset);
                    }
                }
                
               
            }
            else
            {
                if (!NeedCancel)
                {
                    onFinished?.Invoke(false, default,default);
                }
                
                foreach (LoadBundledAssetTask<T> task in MergedTasks)
                {
                    if (!task.NeedCancel)
                    {
                        task.onFinished?.Invoke(false,default,default);
                    }
                }
                
            }
            
           
        }

        #endregion

        /// <summary>
        /// 创建资源加载任务的对象
        /// </summary>
        public static LoadBundledAssetTask<T> Create(TaskRunner owner, string name,
            LoadAssetCallback<T> callback)
        {
            LoadBundledAssetTask<T> task = ReferencePool.Get<LoadBundledAssetTask<T>>();
            task.CreateBase(owner, name);

            task.AssetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.RelativePath);
            task.onFinished = callback;

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            
            onFinished = default;
            AssetRuntimeInfo = default;
            BundleRuntimeInfo = default;
            totalDependencyCount = default;
            loadFinishDependencyCount = default;
            loadBundleAssetState = default;
            Operation = default;
            NeedCancel = default;
        }
    }
}