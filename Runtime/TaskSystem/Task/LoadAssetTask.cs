using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载任务完成回调的原型
    /// </summary>
    public delegate void LoadAssetTaskCallback<in T>(bool success, T asset, object userdata) where T : Object;

    /// <summary>
    /// 资源加载任务
    /// </summary>
    public class LoadAssetTask<T> : BaseTask<LoadAssetTask<T>> where T : Object
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


        protected object Userdata;
        protected LoadAssetTaskCallback<T> OnFinished;


        protected AssetRuntimeInfo AssetRuntimeInfo;
        protected BundleRuntimeInfo BundleRuntimeInfo;

        private readonly LoadBundleTaskCallback onBundleLoadedCallback;

        private int totalDependencyCount;
        private int loadedDependencyCount;
        private readonly LoadAssetTaskCallback<Object> onDependencyLoadedCallback;


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


        /// <inheritdoc />
        public override void Run()
        {
            if (BundleRuntimeInfo.Bundle == null)
            {
                //资源包未加载
                loadAssetState = LoadAssetState.BundleLoading;

                LoadBundleTask task = LoadBundleTask.Create(Owner, BundleRuntimeInfo.Manifest.RelativePath, null,
                    onBundleLoadedCallback);
                Owner.AddTask(task, TaskPriority.Height);
            }
            else
            {
                //资源包已加载
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
        /// 资源包加载结束的回调
        /// </summary>
        private void OnBundleLoaded(bool success, object userdata)
        {
            if (!success)
            {
                //资源包加载失败
                loadAssetState = LoadAssetState.None;
                State = TaskState.Finished;

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");

                OnFinished?.Invoke(false, null, Userdata);
                foreach (LoadAssetTask<T> task in mergedTasks)
                {
                    task.OnFinished?.Invoke(false, null, Userdata);
                }
            }
            else
            {
                //资源包加载成功
                loadAssetState = LoadAssetState.BundleLoaded;
            }
        }

        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(bool success, object asset, object userdata)
        {
            loadedDependencyCount++;

            if (success)
            {
                AssetRuntimeInfo dependencyAssetInfo = CatAssetManager.GetAssetRuntimeInfo(asset);
                BundleRuntimeInfo dependencyBundleInfo =
                    CatAssetManager.GetBundleRuntimeInfo(dependencyAssetInfo.BundleManifest.RelativePath);

                //添加资源的依赖记录
                dependencyAssetInfo.RefAssets.Add(AssetRuntimeInfo);

                if (!dependencyBundleInfo.Equals(BundleRuntimeInfo))
                {
                    //不是同一资源包内资源互相依赖 需要添加资源包的依赖记录
                    dependencyBundleInfo.RefBundles.Add(BundleRuntimeInfo);
                    BundleRuntimeInfo.DependencyBundles.Add(dependencyBundleInfo);
                }
            }
        }

        /// <summary>
        /// 卸载掉加载过的依赖资源
        /// </summary>
        private void UnloadDependencies()
        {
            for (int i = 0; i < AssetRuntimeInfo.AssetManifest.Dependencies.Count; i++)
            {
                string dependencyName = AssetRuntimeInfo.AssetManifest.Dependencies[i];

                AssetRuntimeInfo dependencyInfo = CatAssetManager.GetAssetRuntimeInfo(dependencyName);
                if (dependencyInfo != null && dependencyInfo.Asset != null)
                {
                    //将已加载好的依赖都卸载了
                    dependencyInfo.RefAssets.Remove(AssetRuntimeInfo);
                    CatAssetManager.UnloadAsset(dependencyInfo.Asset);
                }
            }
        }

        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            Operation = BundleRuntimeInfo.Bundle.LoadAssetAsync(Name, AssetRuntimeInfo.AssetManifest.Type);
        }

        /// <summary>
        /// 异步加载结束时调用
        /// </summary>
        protected virtual void OnLoadDone()
        {
            AssetBundleRequest request = (AssetBundleRequest) Operation;
            AssetRuntimeInfo.Asset = request.asset;

            if (AssetRuntimeInfo.Asset != null)
            {
                //添加关联
                CatAssetManager.SetAssetRuntimeInfo(AssetRuntimeInfo.Asset, AssetRuntimeInfo);
            }
        }

        #region 资源加载状态检查

        private void CheckStateWithBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWithBundleLoaded()
        {
            //这里要在资源包加载好后就马上增加资源的引用计数和UsedAssets记录
            //防止在依赖资源加载过程中触发了资源包的卸载
            AssetRuntimeInfo.RefCount++;
            AssetRuntimeInfo.RefCount += mergedTasks.Count;
            BundleRuntimeInfo.UsedAssets.Add(AssetRuntimeInfo);

            if (AssetRuntimeInfo.AssetManifest.Dependencies == null)
            {
                loadAssetState = LoadAssetState.DependenciesLoaded;
                totalDependencyCount = 0;
            }
            else
            {
                //加载依赖
                loadAssetState = LoadAssetState.DependenciesLoading;
                totalDependencyCount = AssetRuntimeInfo.AssetManifest.Dependencies.Count;
                foreach (string dependency in AssetRuntimeInfo.AssetManifest.Dependencies)
                {
                    CatAssetManager.LoadAsset(dependency, null, onDependencyLoadedCallback, TaskPriority.Height);
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

            if (AssetRuntimeInfo.Asset == null)
            {
                //未加载过 发起异步加载
                LoadAsync();
            }
            else
            {
                //已加载过 直接转移到AssetLoaded状态
                loadAssetState = LoadAssetState.AssetLoaded;
            }
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

            if (IsAssetLoadFailed())
            {
                //资源加载失败
                //清空引用计数
                AssetRuntimeInfo.RefCount = 0;
                BundleRuntimeInfo.UsedAssets.Remove(AssetRuntimeInfo);

                //加载过依赖 要卸载依赖
                if (totalDependencyCount > 0)
                {
                    UnloadDependencies();
                }

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");

                OnFinished?.Invoke(false, null, Userdata);
                foreach (LoadAssetTask<T> task in mergedTasks)
                {
                    task.OnFinished?.Invoke(false, null, task.Userdata);
                }

                return;
            }

            //Debug.Log($"资源加载成功：{AssetRuntimeInfo}");

            OnFinished?.Invoke(true, (T) AssetRuntimeInfo.Asset, Userdata);
            foreach (LoadAssetTask<T> task in mergedTasks)
            {
                task.OnFinished?.Invoke(true, (T) AssetRuntimeInfo.Asset, task.Userdata);
            }
        }

        /// <summary>
        /// 资源是否加载失败
        /// </summary>
        protected virtual bool IsAssetLoadFailed()
        {
            return AssetRuntimeInfo.Asset == null;
        }

        #endregion


        /// <summary>
        /// 创建资源加载任务的对象
        /// </summary>
        public static LoadAssetTask<T> Create(TaskRunner owner, string name, object userdata,
            LoadAssetTaskCallback<T> callback)
        {
            LoadAssetTask<T> task = ReferencePool.Get<LoadAssetTask<T>>();
            task.CreateBase(owner, name);

            task.AssetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetManager.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.RelativePath);
            task.Userdata = userdata;
            task.OnFinished = callback;

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            Userdata = default;
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