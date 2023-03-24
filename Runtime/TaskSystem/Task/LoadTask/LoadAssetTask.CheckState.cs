using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    public partial class LoadAssetTask
    {
        /// <summary>
        /// 检查是否已被全部取消
        /// </summary>
        private void CheckAllCanceled()
        {
            if (IsAllCanceled)
            {
                bool flag = false;

                switch (LoadState)
                {
                    case LoadBundledAssetState.BundleNotLoad:
                        flag = true;
                        break;

                    case LoadBundledAssetState.BundleLoading:
                        //取消资源包加载任务
                        flag = true;
                        loadBundleTask.Cancel();
                        break;

                    case LoadBundledAssetState.DependenciesNotLoad:
                        //尝试卸载资源包
                        flag = true;
                        CatAssetManager.TryUnloadBundle(BundleRuntimeInfo);
                        break;

                    case LoadBundledAssetState.DependenciesLoading:
                    case LoadBundledAssetState.AssetNotLoad:
                        //卸载所有依赖
                        flag = true;
                        foreach (var dependencyHandler in dependencyHandlers)
                        {
                            dependencyHandler.Unload();
                        }

                        dependencyHandlers.Clear();

                        //尝试卸载资源包
                        CatAssetManager.TryUnloadBundle(BundleRuntimeInfo);
                        break;
                }

                if (flag)
                {
                    Debug.LogWarning($"{Name}被在{LoadState}阶段被全部取消了");
                    State = TaskState.Finished;
                    LoadState = LoadBundledAssetState.None;
                    CallFinished(false);
                }
            }
        }

        private void CheckStateWhileBundleNotLoad()
        {
            State = TaskState.Waiting;
            LoadState = LoadBundledAssetState.BundleLoading;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                //WebGL平台的资源包加载
                loadBundleTask = LoadWebBundleTask.Create(Owner, BundleRuntimeInfo.LoadPath,
                    BundleRuntimeInfo.Manifest,
                    onBundleLoadedCallback);
            }
            else
            {
                //其他平台的资源包加载
                loadBundleTask = LoadBundleTask.Create(Owner, BundleRuntimeInfo.LoadPath,
                    BundleRuntimeInfo.Manifest,
                    onBundleLoadedCallback);
            }

            Owner.AddTask(loadBundleTask, Group.Priority);
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

            if (AssetRuntimeInfo.Asset == null)
            {
                //未加载过 就在加载依赖前开始记录时间
                startLoadTime = Time.realtimeSinceStartup;
            }

            //加载依赖
            if (AssetRuntimeInfo.AssetManifest.Dependencies == null)
            {
                return;
            }

            totalDependencyCount = AssetRuntimeInfo.AssetManifest.Dependencies.Count;

#if UNITY_EDITOR
            var oldLoader = CatAssetManager.GetAssetLoader();
            if (oldLoader is PriorityEditorUpdatableAssetLoader)
            {
                CatAssetManager.SetAssetLoader<UpdatableAssetLoader>(); //加载资源依赖时 不能优先从编辑器加载 只能从资源包加载
            }
#endif

            foreach (var dependency in AssetRuntimeInfo.AssetManifest.Dependencies)
            {
                AssetHandler<Object> dependencyHandler =
                    CatAssetManager.LoadAssetAsync<Object>(dependency, default, Group.Priority);
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

            if (loadFinishDependencyCount == totalDependencyCount)
            {
                //依赖已加载完
                LoadState = LoadBundledAssetState.DependenciesLoaded;
            }

        }

        private void CheckStateWhileDependenciesLoaded()
        {
            State = TaskState.Waiting;

            if (AssetRuntimeInfo.BundleManifest.IsScene || AssetRuntimeInfo.Asset == null)
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

            if (Operation == null || !Operation.isDone)
            {
                return;
            }

            LoadState = LoadBundledAssetState.AssetLoaded;

            //调用加载结束方法
            LoadDone();

            if (IsLoadFailed())
            {
                return;
            }

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
                if (startLoadTime != 0)
                {
                    //加载成功 计算加载耗时
                    float endLoadTime = Time.realtimeSinceStartup;
                    AssetRuntimeInfo.LoadTime = endLoadTime - startLoadTime;
                    //Debug.Log($"{AssetRuntimeInfo}加载成功，耗时:{AssetRuntimeInfo.LoadTime:0.000}秒");
                }

                //资源加载成功 或 是已加载好的
                CallFinished(true);

                if (IsAllCanceled)
                {
                    //加载成功后所有任务都被取消了 这个资源没人要了 直接走卸载流程吧
                    AssetRuntimeInfo.AddRefCount(); //注意这里要先计数+1 才能正确执行后续的卸载流程
                    CatAssetManager.UnloadAsset(AssetRuntimeInfo.Asset);
                }
            }
        }
    }
}