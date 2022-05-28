

using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载任务完成回调的原型
    /// </summary>
    public delegate void LoadAssetTaskCallback(bool success,object asset,object userdata);
    
    /// <summary>
    /// 资源加载任务
    /// </summary>
    public class LoadAssetTask : BaseTask<LoadAssetTask>
    {
        /// <summary>
        /// 资源加载状态
        /// </summary>
        private enum LoadAssetState
        {
            None,

            /// <summary>
            /// 资源包加载中
            /// </summary>
            BundleLoading,

            /// <summary>
            /// 资源包加载结束
            /// </summary>
            BundleLoaded,

            /// <summary>
            /// 依赖资源加载中
            /// </summary>
            DependenciesLoading,

            /// <summary>
            /// 依赖资源加载结束
            /// </summary>
            DependenciesLoaded,

            /// <summary>
            /// 资源加载中
            /// </summary>
            AssetLoading,

            /// <summary>
            /// 资源加载结束
            /// </summary>
            AssetLoaded,
        }


        private object userdata;
        protected LoadAssetTaskCallback OnFinished;
        

        protected AssetRuntimeInfo AssetRuntimeInfo;
        protected BundleRuntimeInfo BundleRuntimeInfo;
        
        private LoadBundleTaskCallback onBundleLoadedCallback;
        
        private int totalDependencyCount;
        private int loadedDependencyCount;
        private LoadAssetTaskCallback onDependencyLoadedCallback;

      
        private LoadAssetState loadAssetState;
        protected AsyncOperation Operation;

        public LoadAssetTask()
        {
            onBundleLoadedCallback = OnBundleLoaded;
            onDependencyLoadedCallback = OnDependencyLoaded;
        }
        
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
        
        /// <summary>
        /// 资源包加载结束的回调
        /// </summary>
        private void OnBundleLoaded(bool success,object userdata)
        {
            if (!success)
            {
                //资源包加载失败了 直接转移到AssetLoaded状态
                loadAssetState = LoadAssetState.AssetLoaded;
                return;
            }

            loadAssetState = LoadAssetState.BundleLoaded;
        }
        
        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(bool success, object asset,object userdata)
        {
            loadedDependencyCount++;

            if (success)
            {
                AssetRuntimeInfo dependencyRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(asset);
                
                //添加依赖引用记录
                dependencyRuntimeInfo.RefAssetList.Add(Name);
            }
        }
        
        /// <summary>
        /// 卸载掉加载过的依赖资源
        /// </summary>
        protected void UnloadDependencies()
        {
            
            for (int i = 0; i < AssetRuntimeInfo.AssetManifest.Dependencies.Count; i++)
            {
                string dependencyName = AssetRuntimeInfo.AssetManifest.Dependencies[i];

                AssetRuntimeInfo dependencyInfo = CatAssetManager.GetAssetRuntimeInfo(dependencyName);
                if (dependencyInfo != null && dependencyInfo.Asset != null)
                {
                    //将已加载好的依赖都卸载了
                    CatAssetManager.UnloadAsset(dependencyInfo.Asset);
                }

            }
        }
        
        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            Operation = BundleRuntimeInfo.Bundle.LoadAssetAsync(Name);
        }
        
        /// <summary>
        /// 异步加载结束时调用
        /// </summary>
        protected virtual void OnLoadDone()
        {
            AssetBundleRequest request = (AssetBundleRequest)Operation;
            AssetRuntimeInfo.Asset = request.asset;

            if (AssetRuntimeInfo.Asset != null)
            {
                //添加关联
                CatAssetManager.SetAssetRuntimeInfo(AssetRuntimeInfo.Asset,AssetRuntimeInfo);
            }
        }

        #region 资源加载状态检查

          private void CheckStateWithBundleLoading()
        {
            State = TaskState.Waiting;
        }
        
        private void CheckStateWithBundleLoaded()
        {
            //添加引用计数
            AssetRuntimeInfo.RefCount++;
            AssetRuntimeInfo.RefCount += mergedTasks.Count; //要把已合并的任务数量也加到引用计数上
            BundleRuntimeInfo.UsedAssets.Add(Name);
                
            if (AssetRuntimeInfo.AssetManifest.Dependencies == null)
            {
                loadAssetState = LoadAssetState.DependenciesLoaded;
                totalDependencyCount = 0;
            }
            else
            {
                //加载依赖
                loadAssetState =  LoadAssetState.DependenciesLoading;
                totalDependencyCount = AssetRuntimeInfo.AssetManifest.Dependencies.Count;
                foreach (string dependency in AssetRuntimeInfo.AssetManifest.Dependencies)
                {
                    CatAssetManager.LoadAsset(dependency,null, onDependencyLoadedCallback,TaskPriority.Height);
                }
            }
        }

        private bool CheckStateWithDependenciesLoading()
        {
            if (loadedDependencyCount != totalDependencyCount)
            {
                State = TaskState.Waiting;
                return true;
            }
            
            loadAssetState = LoadAssetState.DependenciesLoaded;
            return false;
        }
        
        private void CheckStateWithDependenciesLoaded()
        {
            loadAssetState = LoadAssetState.AssetLoading;
            LoadAsync();
        }
        
        private bool CheckStateWithAssetLoading()
        {
            State = TaskState.Running;
            if (!Operation.isDone)
            {
                return true;
            }

            loadAssetState = LoadAssetState.AssetLoaded;
            OnLoadDone();
            return false;
        }
        
        private void CheckStateWithAssetLoaded()
        {
            State = TaskState.Finished;

            if (BundleRuntimeInfo.Bundle == null)
            {
                //资源包加载失败
                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                
                OnFinished?.Invoke(false,null,userdata);
                foreach (LoadAssetTask task in mergedTasks)
                {
                    task. OnFinished?.Invoke(false,null,userdata);
                }
                
                return;
            }

            if ((!BundleRuntimeInfo.Manifest.IsScene && AssetRuntimeInfo.Asset == null))
            {
                //资源加载失败
                //清空引用计数
                AssetRuntimeInfo.RefCount = 0;
                BundleRuntimeInfo.UsedAssets.Remove(Name);
                    
                //加载过依赖 要卸载依赖
                if (totalDependencyCount > 0)
                {
                    UnloadDependencies();
                }
                    
                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                
                OnFinished?.Invoke(false,null,userdata);
                foreach (LoadAssetTask task in mergedTasks)
                {
                    task. OnFinished?.Invoke(false,null,userdata);
                }
                
                return;
            }
                
            Debug.Log($"资源加载成功：{AssetRuntimeInfo}");
            
            OnFinished?.Invoke(true, AssetRuntimeInfo.Asset,userdata);
            foreach (LoadAssetTask task in mergedTasks)
            {
                task.OnFinished?.Invoke(true, AssetRuntimeInfo.Asset,userdata);
            }
        }

        #endregion
        
        /// <inheritdoc />
        public override void Run()
        {
            if (BundleRuntimeInfo.Bundle == null)
            {
                //需要先加载资源包
                loadAssetState = LoadAssetState.BundleLoading;
                
                LoadBundleTask task = LoadBundleTask.Create(Owner,BundleRuntimeInfo.Manifest.RelativePath,null,onBundleLoadedCallback);
                Owner.AddTask(task, TaskPriority.Height);
            }
            else
            {
                loadAssetState = LoadAssetState.BundleLoaded;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            //1.检查资源包是否加载结束
            if (loadAssetState == LoadAssetState.BundleLoading)
            {
                CheckStateWithBundleLoading();
                return;
            }
            
            //2.资源包加载结束，开始加载依赖资源
            if (loadAssetState == LoadAssetState.BundleLoaded)
            {
                CheckStateWithBundleLoaded();
            }
            
            //3.检查依赖资源是否加载结束
            if (loadAssetState == LoadAssetState.DependenciesLoading)
            {
                bool isReturn = CheckStateWithDependenciesLoading();
                if (isReturn)
                {
                    return;
                }
            }
            
            //4.依赖资源加载结束，开始加载主资源
            if (loadAssetState == LoadAssetState.DependenciesLoaded)
            {
                CheckStateWithDependenciesLoaded();
            }
            
            //5.检查主资源是否加载结束
            if (loadAssetState == LoadAssetState.AssetLoading)
            {
                bool isReturn = CheckStateWithAssetLoading();
                if (isReturn)
                {
                    return;
                }
            }
            
            //6.主资源加载结束，检查是否加载成功
            if (loadAssetState == LoadAssetState.AssetLoaded)
            {
                CheckStateWithAssetLoaded();
            }
        }
        
        /// <summary>
        /// 创建资源加载任务的对象
        /// </summary>
        public static LoadAssetTask Create(TaskRunner owner, string name,object userdata,LoadAssetTaskCallback callback)
        {
            LoadAssetTask task = ReferencePool.Get<LoadAssetTask>();
            task.CreateBase(owner,name);

            task.AssetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetManager.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.RelativePath);
            task.userdata = userdata;
            task.OnFinished = callback;
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            userdata = default;
            OnFinished = default;
            AssetRuntimeInfo = default;
            BundleRuntimeInfo = default;
            totalDependencyCount = default;
            loadedDependencyCount = default;
            loadAssetState = default;
            Operation = default;
        }
    }
}