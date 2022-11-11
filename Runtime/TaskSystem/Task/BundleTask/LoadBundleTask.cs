using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包加载任务完成回调的原型
    /// </summary>
    public delegate void BundleLoadedCallback(bool success);

    /// <summary>
    /// 资源包加载任务
    /// </summary>
    public class LoadBundleTask : BaseTask
    {
        /// <summary>
        /// 资源包加载状态
        /// </summary>
        private enum LoadBundleState
        {
            None,

            /// <summary>
            /// 资源包不存在于本地
            /// </summary>
            BundleNotExist,

            /// <summary>
            /// 资源包下载中
            /// </summary>
            BundleDownloading,

            /// <summary>
            /// 资源包下载结束
            /// </summary>
            BundleDownloaded,

            /// <summary>
            /// 内置Shader资源包未加载
            /// </summary>
            BuiltInShaderBundleNotLoad,

            /// <summary>
            /// 内置Shader资源包加载中
            /// </summary>
            BuiltInShaderBundleLoading,

            /// <summary>
            /// 内置Shader资源包加载结束
            /// </summary>
            BuiltInShaderBundleLoaded,

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

        }

        protected BundleLoadedCallback OnFinishedCallback;
        protected BundleRuntimeInfo BundleRuntimeInfo;

        private readonly BundleUpdatedCallback onBundleUpdatedCallback;
        private readonly BundleLoadedCallback onBuiltInShaderBundleLoadedCallback;

        private LoadBundleState loadState;
        private AssetBundleCreateRequest request;

        /// <inheritdoc />
        public override float Progress
        {
            get
            {
                if (request == null)
                {
                    return 0;
                }

                return request.progress;
            }
        }

        public LoadBundleTask()
        {
            onBundleUpdatedCallback = OnBundleUpdated;
            onBuiltInShaderBundleLoadedCallback = OnBuiltInShaderBundleLoadedCallback;
        }


        /// <inheritdoc />
        public override void Run()
        {
            if (BundleRuntimeInfo.BundleState == BundleRuntimeInfo.State.InRemote)
            {
                //不在本地 需要先下载
                loadState = LoadBundleState.BundleNotExist;
            }
            else
            {
                //在本地了 不需要下载 检查是否需要加载内置Shader资源包
                loadState = LoadBundleState.BundleDownloaded;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (loadState == LoadBundleState.BundleNotExist)
            {
                //资源包不存在于本地
                CheckStateWithBundleNotExist();
            }

            if (loadState == LoadBundleState.BundleDownloading)
            {
                //资源包下载中
                CheckStateWithBundleDownloading();
            }

            if (loadState == LoadBundleState.BundleDownloaded)
            {
                //资源包下载结束
                CheckStateWithBundleDownloaded();
            }

            if (loadState == LoadBundleState.BuiltInShaderBundleNotLoad)
            {
                //内置Shader资源包未加载
                CheckStateWithBuiltInShaderBundleNotLoad();
            }

            if (loadState == LoadBundleState.BuiltInShaderBundleLoading)
            {
                //内置Shader资源包加载中
                CheckStateWithBuiltInShaderBundleLoading();
            }

            if (loadState == LoadBundleState.BuiltInShaderBundleLoaded)
            {
                //内置Shader资源包加载结束
                CheckStateWithBuiltInShaderBundleLoaded();
            }

            if (loadState == LoadBundleState.BundleNotLoad)
            {
                //资源包未加载
                CheckStateWithBundleNotLoad();
            }

            if (loadState == LoadBundleState.BundleLoading)
            {
                //资源包加载中
                CheckStateWithBundleLoading();
            }

            if (loadState == LoadBundleState.BundleLoaded)
            {
                //资源包加载结束
                CheckStateWithBundleLoaded();
            }
        }

        /// <summary>
        /// 资源包更新完毕回调
        /// </summary>
        private void OnBundleUpdated(BundleUpdateResult result)
        {
            if (!result.Success)
            {
                //下载失败
                loadState = LoadBundleState.BundleLoaded;
                return;
            }

            //下载成功 检测是否需要加载内置Shader资源包
            Debug.Log($"下载成功：{result.BundleRelativePath}");
            loadState = LoadBundleState.BundleDownloaded;
        }

        /// <summary>
        /// 内置Shader资源包加载完毕回调
        /// </summary>
        private void OnBuiltInShaderBundleLoadedCallback(bool success)
        {
            loadState = LoadBundleState.BuiltInShaderBundleLoaded;
        }

        private void CheckStateWithBundleNotExist()
        {
            State = TaskState.Waiting;
            loadState = LoadBundleState.BundleDownloading;

            //下载本地不存在的资源包
            Debug.Log($"开始下载：{BundleRuntimeInfo.Manifest.RelativePath}");
            CatAssetManager.UpdateBundle(BundleRuntimeInfo.Manifest.Group,BundleRuntimeInfo.Manifest,onBundleUpdatedCallback);
        }

        private void CheckStateWithBundleDownloading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWithBundleDownloaded()
        {
            State = TaskState.Waiting;

            if (BundleRuntimeInfo.Manifest.IsDependencyBuiltInShaderBundle)
            {
                //此资源包依赖内置Shader资源包
                BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
                if (builtInShaderBundleRuntimeInfo.Bundle == null)
                {
                    //内置Shader资源包未加载 需要加载
                    loadState = LoadBundleState.BuiltInShaderBundleNotLoad;
                }
                else
                {
                    //内置Shader资源包已加载 添加依赖链记录
                    loadState = LoadBundleState.BuiltInShaderBundleLoaded;
                }
            }
            else
            {
                //不需要加载内置Shader资源包
                loadState = LoadBundleState.BundleNotLoad;
            }
        }

        private void CheckStateWithBuiltInShaderBundleNotLoad()
        {
            State = TaskState.Waiting;
            loadState = LoadBundleState.BuiltInShaderBundleLoading;

            BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
            BaseTask task;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                task = LoadWebBundleTask.Create(Owner, builtInShaderBundleRuntimeInfo.Manifest.RelativePath,
                    onBuiltInShaderBundleLoadedCallback);
            }
            else
            {
                task = Create(Owner, builtInShaderBundleRuntimeInfo.Manifest.RelativePath,
                    onBuiltInShaderBundleLoadedCallback);
            }
            Owner.AddTask(task, TaskPriority.Middle);
        }

        private void CheckStateWithBuiltInShaderBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWithBuiltInShaderBundleLoaded()
        {
            State = TaskState.Waiting;
            loadState = LoadBundleState.BundleNotLoad;


            BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
            if (builtInShaderBundleRuntimeInfo.Bundle != null)
            {
                //加载成功 添加依赖链记录
                builtInShaderBundleRuntimeInfo.DependencyChain.DownStream.Add(BundleRuntimeInfo);
                BundleRuntimeInfo.DependencyChain.UpStream.Add(builtInShaderBundleRuntimeInfo);
            }

        }

        private void CheckStateWithBundleNotLoad()
        {
            State = TaskState.Running;
            loadState = LoadBundleState.BundleLoading;

            LoadAsync();
        }

        private void CheckStateWithBundleLoading()
        {
            State = TaskState.Running;

            if (IsLoadDone())
            {
                loadState = LoadBundleState.BundleLoaded;

                LoadDone();
            }
        }

        private void CheckStateWithBundleLoaded()
        {
            State = TaskState.Finished;

            if (BundleRuntimeInfo.Bundle == null)
            {
                Debug.LogError($"资源包加载失败：{BundleRuntimeInfo.Manifest}");
                OnFinishedCallback?.Invoke(false);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.OnFinishedCallback?.Invoke(false);
                }
            }
            else
            {
                //Debug.Log($"资源包加载成功：{bundleRuntimeInfo.Manifest}");
                OnFinishedCallback?.Invoke(true);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.OnFinishedCallback?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            request = AssetBundle.LoadFromFileAsync(BundleRuntimeInfo.LoadPath);
        }

        /// <summary>
        /// 是否异步加载结束
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsLoadDone()
        {
            return request.isDone;
        }

        /// <summary>
        /// 异步加载结束
        /// </summary>
        protected virtual void LoadDone()
        {
            BundleRuntimeInfo.Bundle = request.assetBundle;
        }

        /// <summary>
        /// 创建资源包加载任务的对象
        /// </summary>
        public static LoadBundleTask Create(TaskRunner owner, string name,BundleLoadedCallback callback)
        {
            LoadBundleTask task = ReferencePool.Get<LoadBundleTask>();
            task.CreateBase(owner,name);

            task.OnFinishedCallback = callback;
            task.BundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(name);

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            OnFinishedCallback = default;
            BundleRuntimeInfo = default;
            request = default;
            loadState = default;
        }
    }
}
