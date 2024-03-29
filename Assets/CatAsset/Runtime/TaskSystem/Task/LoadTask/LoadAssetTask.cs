﻿using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包资源加载任务
    /// </summary>
    public partial class LoadAssetTask : BaseTask
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

        private BaseTask loadBundleTask;
        private readonly BundleLoadedCallback onBundleLoadedCallback;

        private int totalDependencyCount;
        private int loadFinishDependencyCount;
        private readonly List<AssetHandler> dependencyHandlers = new List<AssetHandler>();
        private readonly AssetLoadedCallback<Object> onDependencyLoadedCallback;

        protected LoadBundledAssetState LoadState;
        protected AsyncOperation Operation;

        private float startLoadTime;

        /// <inheritdoc />
        public override string SubState => LoadState.ToString();
        
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
        
        public LoadAssetTask()
        {
            onBundleLoadedCallback = OnBundleLoaded;
            onDependencyLoadedCallback = OnDependencyLoaded;
        }


        /// <inheritdoc />
        public override void Run()
        {
            //这里引用计数先额外+1 防止加载资源过程中 资源包被意外卸载了 
            AssetRuntimeInfo.AddRefCount(); 
            
            if (BundleRuntimeInfo.Bundle == null)
            {
                //资源包未加载
                LoadState = LoadBundledAssetState.BundleNotLoad;
            }
            else
            {
                //资源包已加载
                //但资源未加载或已加载但未被引用 加载一遍依赖
                LoadState = LoadBundledAssetState.DependenciesNotLoad;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            //检查是否已被全部取消
            CheckAllCanceled();

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
        public override void OnPriorityChanged()
        {
            //修改加载资源包的优先级
            if (loadBundleTask != null)
            {
                loadBundleTask.Owner.ChangePriority(loadBundleTask.MainTask, Group.Priority);
            }

            //修改加载依赖资源的优先级
            foreach (AssetHandler dependencyHandler in dependencyHandlers)
            {
                dependencyHandler.Priority = Group.Priority;
            }

            //修改加载资源的优先级
            if (Operation != null)
            {
                Operation.priority = (int)Group.Priority;
            }
        }

        /// <summary>
        /// 资源包加载结束的回调
        /// </summary>
        private void OnBundleLoaded(bool success)
        {
            LoadState = LoadBundledAssetState.BundleLoaded;
            loadBundleTask = null;
        }

        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(AssetHandler<Object> _)
        {
            loadFinishDependencyCount++;
        }


        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            Operation = BundleRuntimeInfo.Bundle.LoadAssetAsync(Name, assetType);
            Operation.priority = (int)Group.Priority;
        }

        /// <summary>
        /// 异步加载结束
        /// </summary>
        protected virtual void LoadDone()
        {
            AssetBundleRequest request = (AssetBundleRequest)Operation;
            AssetRuntimeInfo.Asset = request.asset;

            if (AssetRuntimeInfo.Asset != null)
            {
                //获取内存占用
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
            }

            if (success && IsAllCanceled)
            {
                //加载资源成功后所有任务都被取消了 这个资源没人要了 直接走卸载流程吧
                CatAssetManager.UnloadAsset(AssetRuntimeInfo.Asset);
                return;
            }

            //加载失败 或者 加载成功且没有全部取消 就把之前额外增加的引用计数减掉
            AssetRuntimeInfo.SubRefCount();

            foreach (LoadAssetTask task in MergedTasks)
            {
                if (!task.IsCanceled)
                {
                    if (success)
                    {
                        //成功
                        AssetRuntimeInfo.AddRefCount();
                        task.handler.SetAsset(AssetRuntimeInfo.Asset);
                    }
                    else
                    {
                        //失败
                        task.handler.SetAsset(null);
                    }
                }
                else
                {
                    //已取消
                    task.handler.NotifyCanceled(task.CancelToken);
                }
            }
            

        }


        /// <summary>
        /// 创建资源加载任务的对象
        /// </summary>
        public static LoadAssetTask Create(TaskRunner owner, string name, Type assetType, AssetHandler handler,
            CancellationToken token = default)
        {
            LoadAssetTask task = ReferencePool.Get<LoadAssetTask>();
            task.CreateBase(owner, name, token);

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

            loadBundleTask = default;

            //释放掉所有依赖资源的句柄
            foreach (AssetHandler dependencyHandler in dependencyHandlers)
            {
                dependencyHandler.Release();
            }

            dependencyHandlers.Clear();

            LoadState = default;
            Operation = default;

            startLoadTime = default;
        }
    }
}
