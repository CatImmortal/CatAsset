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
                //在本地了
                loadState = LoadBundleState.BundleDownloaded;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (loadState == LoadBundleState.BundleNotExist)
            {
                //资源包不存在于本地
                CheckStateWhileBundleNotExist();
            }

            if (loadState == LoadBundleState.BundleDownloading)
            {
                //资源包下载中
                CheckStateWhileBundleDownloading();
            }

            if (loadState == LoadBundleState.BundleDownloaded)
            {
                //资源包下载结束
                CheckStateWhileBundleDownloaded();
            }

            if (loadState == LoadBundleState.BundleNotLoad)
            {
                //资源包未加载
                CheckStateWhileBundleNotLoad();
            }

            if (loadState == LoadBundleState.BundleLoading)
            {
                //资源包加载中
                CheckStateWhileBundleLoading();
            }

            if (loadState == LoadBundleState.BundleLoaded)
            {
                //资源包加载结束
                CheckStateWhileBundleLoaded();
            }
            
            if (loadState == LoadBundleState.BuiltInShaderBundleNotLoad)
            {
                //内置Shader资源包未加载
                CheckStateWhileBuiltInShaderBundleNotLoad();
            }

            if (loadState == LoadBundleState.BuiltInShaderBundleLoading)
            {
                //内置Shader资源包加载中
                CheckStateWhileBuiltInShaderBundleLoading();
            }

            if (loadState == LoadBundleState.BuiltInShaderBundleLoaded)
            {
                //内置Shader资源包加载结束
                CheckStateWhileBuiltInShaderBundleLoaded();
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
            Debug.Log($"下载成功：{result.UpdateInfo.Info}");
            loadState = LoadBundleState.BundleDownloaded;
        }

        /// <summary>
        /// 内置Shader资源包加载完毕回调
        /// </summary>
        private void OnBuiltInShaderBundleLoadedCallback(bool success)
        {
            loadState = LoadBundleState.BuiltInShaderBundleLoaded;
        }

        private void CheckStateWhileBundleNotExist()
        {
            State = TaskState.Waiting;
            loadState = LoadBundleState.BundleDownloading;

            //下载本地不存在的资源包
            Debug.Log($"开始下载：{BundleRuntimeInfo.Manifest.RelativePath}");
            CatAssetManager.UpdateBundle(BundleRuntimeInfo.Manifest.Group,BundleRuntimeInfo.Manifest,onBundleUpdatedCallback);
        }

        private void CheckStateWhileBundleDownloading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWhileBundleDownloaded()
        {
            State = TaskState.Waiting;
            loadState = LoadBundleState.BundleNotLoad;
        }


        private void CheckStateWhileBundleNotLoad()
        {
            State = TaskState.Running;
            loadState = LoadBundleState.BundleLoading;

            LoadAsync();
        }

        private void CheckStateWhileBundleLoading()
        {
            State = TaskState.Running;

            if (IsLoadDone())
            {
                loadState = LoadBundleState.BundleLoaded;
                LoadDone();
            }
        }

        private void CheckStateWhileBundleLoaded()
        {
            if (BundleRuntimeInfo.Bundle == null)
            {
                //加载失败
                State = TaskState.Finished;
                CallFinished(false);
            }
            else
            {
                //加载成功
                
                if (!BundleRuntimeInfo.Manifest.IsDependencyBuiltInShaderBundle)
                {
                    //不依赖内置Shader资源包 直接结束
                    State = TaskState.Finished;
                    CallFinished(true);
                }
                else
                {
                    State = TaskState.Waiting;
                    
                    BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
                    if (builtInShaderBundleRuntimeInfo.Bundle != null)
                    {
                        //依赖内置Shader资源包 但其已加载过了 直接添加依赖链记录
                        loadState = LoadBundleState.BuiltInShaderBundleLoaded;
                    }
                    else
                    {
                        //加载内置Shader资源包
                        loadState = LoadBundleState.BuiltInShaderBundleNotLoad;
                    }
                }
                
            }
        }

        private void CheckStateWhileBuiltInShaderBundleNotLoad()
        {
            State = TaskState.Waiting;
            loadState = LoadBundleState.BuiltInShaderBundleLoading;

            BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
            BaseTask task;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                task = LoadWebBundleTask.Create(Owner, builtInShaderBundleRuntimeInfo.LoadPath,
                    BundleRuntimeInfo.Manifest.RelativePath,
                    onBuiltInShaderBundleLoadedCallback);
            }
            else
            {
                task = Create(Owner, builtInShaderBundleRuntimeInfo.LoadPath,
                    builtInShaderBundleRuntimeInfo.Manifest.RelativePath,
                    onBuiltInShaderBundleLoadedCallback);
            }
            Owner.AddTask(task, TaskPriority.Middle);
        }

        private void CheckStateWhileBuiltInShaderBundleLoading()
        {
            State = TaskState.Waiting;
        }

        private void CheckStateWhileBuiltInShaderBundleLoaded()
        {
            State = TaskState.Finished;

            BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(RuntimeUtil.BuiltInShaderBundleName);
            if (builtInShaderBundleRuntimeInfo.Bundle != null)
            {
                //加载成功 添加依赖链记录
                builtInShaderBundleRuntimeInfo.DependencyChain.DownStream.Add(BundleRuntimeInfo);
                BundleRuntimeInfo.DependencyChain.UpStream.Add(builtInShaderBundleRuntimeInfo);
            }
            
            //通知主资源包加载结束
            CallFinished(true);
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
        /// 调用加载完毕回调
        /// </summary>
        private void CallFinished(bool success)
        {
            if (!success)
            {
                Debug.LogError($"资源包加载失败：{BundleRuntimeInfo.Manifest}");
            }

            
            OnFinishedCallback?.Invoke(success);
            foreach (LoadBundleTask task in MergedTasks)
            {
                task.OnFinishedCallback?.Invoke(success);
            }
        }

        /// <summary>
        /// 创建资源包加载任务的对象
        /// </summary>
        public static LoadBundleTask Create(TaskRunner owner, string name,string bundleRelativePath,BundleLoadedCallback callback)
        {
            LoadBundleTask task = ReferencePool.Get<LoadBundleTask>();
            task.CreateBase(owner,name);

            task.OnFinishedCallback = callback;
            task.BundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(bundleRelativePath);

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
