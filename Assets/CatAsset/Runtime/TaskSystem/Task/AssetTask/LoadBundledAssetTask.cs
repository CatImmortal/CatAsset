using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
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

        /// <summary>
        /// 是否被取消，handler为空 就认为此任务被取消了
        /// </summary>
        protected virtual bool IsCanceled => handler == null;

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
                CheckStateWhileBundleNotLoad();
            }

            if (LoadState == LoadBundledAssetState.BundleLoading)
            {
                //资源包加载中
                CheckStateWhileBundleLoading();
            }

            if (LoadState == LoadBundledAssetState.BundleLoaded)
            {
                //资源包加载结束
                CheckStateWhileBundleLoaded();
            }

            if (LoadState == LoadBundledAssetState.DependenciesNotLoad)
            {
                //依赖资源未加载
                CheckStateWhileDependenciesNotLoad();
            }

            if (LoadState == LoadBundledAssetState.DependenciesLoading)
            {
                //依赖资源加载中
                CheckStateWhileDependenciesLoading();
            }

            if (LoadState == LoadBundledAssetState.DependenciesLoaded)
            {
                //依赖资源加载结束
                CheckStateWhileDependenciesLoaded();
            }

            if (LoadState == LoadBundledAssetState.AssetNotLoad)
            {
                //资源未加载
                CheckStateWhileAssetNotLoad();
            }

            if (LoadState == LoadBundledAssetState.AssetLoading)
            {
                //资源加载中
                CheckStateWhileAssetLoading();
            }

            if (LoadState == LoadBundledAssetState.AssetLoaded)
            {
                //资源加载结束
                CheckStateWhileAssetLoaded();
            }
        }

        /// <inheritdoc />
        public override void Cancel()
        {
            handler = null;
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


        private void CheckStateWhileBundleNotLoad()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundledAssetState.BundleLoading;

            BaseTask task;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                //WebGL平台的资源包加载
                task = LoadWebBundleTask.Create(Owner, BundleRuntimeInfo.LoadPath,
                    BundleRuntimeInfo.Manifest,
                    onBundleLoadedCallback);
            }
            else
            {
                //其他平台的资源包加载
                task = LoadBundleTask.Create(Owner, BundleRuntimeInfo.LoadPath,
                    BundleRuntimeInfo.Manifest,
                    onBundleLoadedCallback);
            }
            Owner.AddTask(task, TaskPriority.Middle);
        }

        private void CheckStateWhileBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWhileBundleLoaded()
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

        private void CheckStateWhileDependenciesNotLoad()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundledAssetState.DependenciesLoading;

            //加载依赖
            totalDependencyCount = AssetRuntimeInfo.AssetManifest.Dependencies.Count;

            if (totalDependencyCount == 0)
            {
                return;
            }

#if UNITY_EDITOR
            var oldLoader = CatAssetManager.GetAssetLoader();
            if (oldLoader is PriorityEditorUpdatableAssetLoader)
            {
                CatAssetManager.SetAssetLoader<UpdatableAssetLoader>();  //加载资源依赖时 不能优先从编辑器加载 只能从资源包加载
            }
#endif

            foreach (var dependency in AssetRuntimeInfo.AssetManifest.Dependencies)
            {
                AssetHandler<Object> dependencyHandler = CatAssetManager.LoadAssetAsync<Object>(dependency,default,TaskPriority.Middle);
                dependencyHandler.OnLoaded += onDependencyLoadedCallback;
                dependencyHandlers.Add(dependencyHandler);
            }

#if UNITY_EDITOR
            CatAssetManager.SetAssetLoader(oldLoader.GetType());
#endif

        }

        private void CheckStateWhileDependenciesLoading()
        {
            State = TaskState.Waiting;

            if (loadFinishDependencyCount != totalDependencyCount)
            {
                //依赖还未加载完
                return;
            }

            LoadState = LoadBundledAssetState.DependenciesLoaded;
        }

        private void CheckStateWhileDependenciesLoaded()
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

        private void CheckStateWhileAssetNotLoad()
        {
            State = TaskState.Running;
            LoadState = LoadBundledAssetState.AssetLoading;

            LoadAsync();
        }

        private void CheckStateWhileAssetLoading()
        {
            State = TaskState.Running;

            if (Operation != null && Operation.isDone)
            {
                LoadState = LoadBundledAssetState.AssetLoaded;

                //调用加载结束方法
                LoadDone();

                if (!IsLoadFailed())
                {
                    //成功加载资源到内存中
                    //添加依赖链记录
                    foreach (AssetHandler dependencyHandler in dependencyHandlers)
                    {
                        if (!dependencyHandler.IsSuccess)
                        {
                            continue;
                        }

                        AssetRuntimeInfo depInfo = CatAssetDatabase.GetAssetRuntimeInfo(dependencyHandler.AssetObj);

                        //更新自身与依赖资源的上下游关系
                        depInfo.DependencyChain.DownStream.Add(AssetRuntimeInfo);
                        depInfo.DownStreamRecord.Add(AssetRuntimeInfo);
                        AssetRuntimeInfo.DependencyChain.UpStream.Add(depInfo);

                        //如果依赖了其他资源包里的资源 还需要设置 自身所在资源包 与 依赖所在资源包 的上下游关系
                        if (!depInfo.BundleManifest.Equals(AssetRuntimeInfo.BundleManifest))
                        {
                            BundleRuntimeInfo depBundleInfo =
                                CatAssetDatabase.GetBundleRuntimeInfo(depInfo.BundleManifest.BundleIdentifyName);

                            depBundleInfo.DependencyChain.DownStream.Add(BundleRuntimeInfo);
                            BundleRuntimeInfo.DependencyChain.UpStream.Add(depBundleInfo);
                        }
                    }
                }
            }
        }

        private void CheckStateWhileAssetLoaded()
        {
            State = TaskState.Finished;

            if (IsLoadFailed())
            {
                //资源加载失败
                //将依赖资源的句柄都卸载一遍
                foreach (AssetHandler dependencyHandler in dependencyHandlers)
                {
                    dependencyHandler.Unload();
                }
                dependencyHandlers.Clear();

                //尝试卸载资源包
                CatAssetManager.TryUnloadBundle(BundleRuntimeInfo);

                CallFinished(false);
            }
            else
            {
                //资源加载成功 或 是已加载好的
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

                AssetRuntimeInfo.MemorySize = (ulong)Profiler.GetRuntimeMemorySizeLong((Object)AssetRuntimeInfo.Asset);

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
                handler.Error = "资源加载失败";

                //加载失败 通知所有未取消的加载任务
                if (!IsCanceled)
                {
                    handler.SetAsset(null);
                }

                foreach (LoadBundledAssetTask task in MergedTasks)
                {
                    if (!task.IsCanceled)
                    {
                        task.handler.SetAsset(null);
                    }
                }
            }
            else
            {
                if (IsAllCanceled())
                {
                    //所有任务都被取消了 这个资源没人要了 直接走卸载流程吧
                    AssetRuntimeInfo.AddRefCount();  //注意这里要先计数+1 才能正确执行后续的卸载流程
                    CatAssetManager.UnloadAsset(AssetRuntimeInfo.Asset);
                    return;
                }

                //加载成功 通知所有未取消的加载任务
                if (!IsCanceled)
                {
                    AssetRuntimeInfo.AddRefCount();
                    handler.SetAsset(AssetRuntimeInfo.Asset);
                }
                foreach (LoadBundledAssetTask task in MergedTasks)
                {
                    if (!task.IsCanceled)
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
        private bool IsAllCanceled()
        {
            foreach (LoadBundledAssetTask task in MergedTasks)
            {
                if (!task.IsCanceled)
                {
                    return false;
                }
            }

            return IsCanceled;
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
                CatAssetDatabase.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.BundleIdentifyName);


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
        }
    }
}
