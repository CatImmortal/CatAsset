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
            
            //这里不能直接检查资源是否已加载好 因为还需要添加资源引用计数以及为依赖资源递归添加引用计数
            //所以最多只能转移到BundleLoaded阶段
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

        /// <inheritdoc />
        public override void Cancel()
        {
            NeedCancel = true;
            Debug.Log("Cancel：" + GUID);
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

                //添加依赖资源的被引用记录
                dependencyAssetInfo.RefAssets.Add(AssetRuntimeInfo);

                if (!dependencyBundleInfo.Equals(BundleRuntimeInfo))
                {
                    //依赖了其他资源包的资源 需要添加被引用资源包的引用记录，和所属资源包的依赖记录
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
            if (totalDependencyCount == 0)
            {
                return;
            }
            
            foreach (string dependencyName in AssetRuntimeInfo.AssetManifest.Dependencies)
            {
                AssetRuntimeInfo dependencyInfo = CatAssetManager.GetAssetRuntimeInfo(dependencyName);
                if (dependencyInfo.Asset != null)
                {
                    //将已加载的依赖都卸载一遍
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
                    //对自身和已加载的依赖资源递归增加 已合并任务数量的引用计数
                    //保证1次LoadAsset一定增加1个自身和所有已加载依赖资源的引用计数
                    AddDependencyRefCount(Name,MergedTaskCount);
                
                    onFinished?.Invoke(true, (T) AssetRuntimeInfo.Asset, Userdata);
                    foreach (LoadAssetTask<T> task in mergedTasks)
                    {
                        if (!task.NeedCancel)
                        {
                            task.onFinished?.Invoke(true, (T) AssetRuntimeInfo.Asset, task.Userdata);
                        }
                   
                    }
                }
                else
                {
                    //被取消了 卸载掉加载成功的资源
                    CatAssetManager.UnloadAsset(AssetRuntimeInfo.Asset);
                
                    //只是主任务被取消了 未取消的已合并任务还需要继续运行
                    foreach (LoadAssetTask<T> task in mergedTasks)
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
                    onFinished?.Invoke(false,default, Userdata);
                    foreach (LoadAssetTask<T> task in mergedTasks)
                    {
                        task.onFinished?.Invoke(false,default, task.Userdata);
                    }
                }
                
            }
            
           
        }

        /// <summary>
        /// 为资源及其依赖资源递归添加引用计数
        /// </summary>
        private void AddDependencyRefCount(string assetName, int count)
        {
            AssetRuntimeInfo info = CatAssetManager.GetAssetRuntimeInfo(assetName);
            if (info.Asset == null)
            {
                //没加载好的资源就不添加引用计数了
                return;
            }
            
            if (info.AssetManifest.Dependencies != null)
            {
                foreach (string dependencyName in info.AssetManifest.Dependencies)
                {
                    AddDependencyRefCount(dependencyName,count);
                }                
            }

            info.RefCount += count;
        }
        
        #region 资源加载状态检查

        private void CheckStateWithBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWithBundleLoaded()
        {
            //这里要在资源包加载好后就马上增加资源的引用计数和使用记录
            //防止在依赖资源加载过程中意外的进行了资源包的卸载
            AssetRuntimeInfo.RefCount++;
            BundleRuntimeInfo.UsedAssets.Add(AssetRuntimeInfo);

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

        private bool CheckStateWithDependenciesLoading()
        {
            if (loadFinishDependencyCount != totalDependencyCount)
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
                //只对普通资源这样复用
                //场景资源每次加载都相当于重新Instantiate了，没有复用的概念
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
            LoadDone();

            return false;
        }

        private void CheckStateWithAssetLoaded()
        {
            State = TaskState.Finished;

            if (IsLoadFailed())
            {
                //资源加载失败
                
                //清空引用计数，删除使用记录
                AssetRuntimeInfo.RefCount = 0;
                BundleRuntimeInfo.UsedAssets.Remove(AssetRuntimeInfo);

                //卸载已加载好的依赖 清除引用记录
                UnloadDependencies();

                Debug.LogError($"资源加载失败：{AssetRuntimeInfo}");

                CallFinished(false);
                return;
            }

            //Debug.Log($"资源加载成功：{AssetRuntimeInfo}");
            CallFinished(true);
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