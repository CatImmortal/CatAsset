using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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
        
        protected object Userdata;
        private LoadAssetTaskCallback<T> onFinished;


        protected AssetRuntimeInfo AssetRuntimeInfo;
        protected BundleRuntimeInfo BundleRuntimeInfo;
        
        private LoadBundleTaskCallback onBundleLoadedCallback;

        private int totalDependencyCount;
        private int loadFinishDependencyCount;
        private LoadAssetTaskCallback<Object> onDependencyLoadedCallback;


        private LoadAssetState loadAssetState;
        protected AsyncOperation Operation;

        protected bool NeedCancel;

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
            if (AssetRuntimeInfo.RefCount > 0)
            {
                //资源已加载好 且正在使用中
                //无需考虑操作依赖资源的引用计数与引用记录
                //直接增加自身引用计数然后转移到DependenciesLoaded状态即可
                AssetRuntimeInfo.AddRefCount();
                loadAssetState = LoadAssetState.DependenciesLoaded;
                return;
            }
            
            if (BundleRuntimeInfo.Bundle == null)
            {
                //资源包未加载
                LoadBundleTask task = LoadBundleTask.Create(Owner, BundleRuntimeInfo.Manifest.RelativePath, null,
                    onBundleLoadedCallback);
                Owner.AddTask(task, TaskPriority.Height);
                
                loadAssetState = LoadAssetState.BundleLoading;
                return;
            }
            
            //资源包已加载
            //但资源未加载或未使用
            loadAssetState = LoadAssetState.BundleLoaded;
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (loadAssetState)
            {
                case LoadAssetState.BundleLoading:
                    //1.检查资源包是否加载结束
                    CheckStateWithBundleLoading();
                    break;
                
                case LoadAssetState.BundleLoaded:
                    //2.资源包加载结束，开始加载依赖资源
                    CheckStateWithBundleLoaded();
                    break;
                
                case LoadAssetState.DependenciesLoading:
                    //3.检查依赖资源是否加载结束
                    CheckStateWithDependenciesLoading();
                    break;
                
                case LoadAssetState.DependenciesLoaded:
                    //4.依赖资源加载结束，开始加载主资源
                    CheckStateWithDependenciesLoaded();
                    break;
                
                case LoadAssetState.AssetLoading:
                    //5.检查主资源是否加载结束
                    CheckStateWithAssetLoading();
                    break;
                
                case LoadAssetState.AssetLoaded:
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
                loadAssetState = LoadAssetState.None;
                State = TaskState.Finished;

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                CallFinished(false);

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
            loadFinishDependencyCount++;

            if (success)
            {
                AssetRuntimeInfo dependencyAssetInfo = CatAssetManager.GetAssetRuntimeInfo(asset);
                BundleRuntimeInfo dependencyBundleInfo =
                    CatAssetManager.GetBundleRuntimeInfo(dependencyAssetInfo.BundleManifest.RelativePath);

                //添加到依赖资源的被引用记录
                dependencyAssetInfo.AddRefAsset(AssetRuntimeInfo);
                
                if (!dependencyBundleInfo.Equals(BundleRuntimeInfo))
                {
                    //依赖了其他资源包的资源 需要添加到依赖资源包的被引用记录中，以及所属资源包的依赖记录中
                    dependencyBundleInfo.AddRefBundle(BundleRuntimeInfo);
                    BundleRuntimeInfo.AddDependencyBundle(dependencyBundleInfo);
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
            //防止在依赖资源加载过程中意外的触发了资源包的卸载
            AssetRuntimeInfo.AddRefCount();

            if (AssetRuntimeInfo.AssetManifest.Dependencies == null)
            {
                //没有依赖需要加载
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

        private void CheckStateWithDependenciesLoading()
        {
            State = TaskState.Waiting;
            
            if (loadFinishDependencyCount != totalDependencyCount)
            {
                return;
            }

            loadAssetState = LoadAssetState.DependenciesLoaded;
        }

        private void CheckStateWithDependenciesLoaded()
        {
            State = TaskState.Running;
            
            if (AssetRuntimeInfo.Asset == null)
            {
                //未加载过 发起异步加载
                //只对普通资源这样复用
                //场景资源每次加载都相当于重新Instantiate了，没有复用的概念
                LoadAsync();
                loadAssetState = LoadAssetState.AssetLoading;
            }
            else
            {
                //已加载过 直接转移到AssetLoaded状态
                loadAssetState = LoadAssetState.AssetLoaded;
            }
        }

        private void CheckStateWithAssetLoading()
        {
            if (!Operation.isDone)
            {
                State = TaskState.Running;
                return;
            }

            loadAssetState = LoadAssetState.AssetLoaded;
            LoadDone();
        }

        private void CheckStateWithAssetLoaded()
        {
            State = TaskState.Finished;

            if (IsLoadFailed())
            {
                //资源加载失败
                
                //清空引用计数，删除使用记录
                AssetRuntimeInfo.SubRefCount();
                
                //卸载已加载好的依赖 清除引用记录
                if (totalDependencyCount > 0)
                {
                    foreach (string dependencyName in AssetRuntimeInfo.AssetManifest.Dependencies)
                    {
                        AssetRuntimeInfo dependencyInfo = CatAssetManager.GetAssetRuntimeInfo(dependencyName);
                        if (dependencyInfo.Asset != null)
                        {
                            //将已加载的依赖都卸载一遍
                            dependencyInfo.RemoveRefAsset(AssetRuntimeInfo);
                            CatAssetManager.UnloadAsset(dependencyInfo.Asset);
                        }
                    }
                }
                

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");
                CallFinished(false);
                
                return;
            }

            //Debug.Log($"资源加载成功：{AssetRuntimeInfo}");
            CallFinished(true);
        }
        
        #endregion

        
        #region 给子类重写的虚方法

        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            Operation = BundleRuntimeInfo.Bundle.LoadAssetAsync(Name, AssetRuntimeInfo.AssetManifest.Type);
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
                CatAssetManager.SetAssetInstance(AssetRuntimeInfo.Asset, AssetRuntimeInfo);
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
                if (!NeedCancel)
                {
                    onFinished?.Invoke(true, (T) AssetRuntimeInfo.Asset, Userdata);
                    foreach (LoadAssetTask<T> task in MergedTasks)
                    {
                        if (!task.NeedCancel)
                        {
                            //对自身增加 已合并且未取消任务数量的引用计数
                            //保证1次成功的LoadAsset一定增加1个自身的引用计数
                            AssetRuntimeInfo.AddRefCount();
                            task.onFinished?.Invoke(true, (T) AssetRuntimeInfo.Asset, task.Userdata);
                        }
                   
                    }
                }
                else
                {
                    //被取消了 卸载掉加载成功的资源
                    CatAssetManager.UnloadAsset(AssetRuntimeInfo.Asset);
                
                    //只是主任务被取消了 未取消的已合并任务还需要继续运行
                    foreach (LoadAssetTask<T> task in MergedTasks)
                    {
                        if (!task.NeedCancel)
                        {
                            CatAssetManager.LoadAsset(task.Name, task.Userdata, task.onFinished);
                        }
                    }
                }
                
               
            }
            else
            {
                if (!NeedCancel)
                {
                    onFinished?.Invoke(false,null, Userdata);
                    foreach (LoadAssetTask<T> task in MergedTasks)
                    {
                        task.onFinished?.Invoke(false,null, task.Userdata);
                    }
                }
                
            }
            
           
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
            task.onFinished = callback;

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            Userdata = default;
            onFinished = default;
            AssetRuntimeInfo = default;
            BundleRuntimeInfo = default;
            totalDependencyCount = default;
            loadFinishDependencyCount = default;
            loadAssetState = default;
            Operation = default;
            NeedCancel = default;
        }
    }
}