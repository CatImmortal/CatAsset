using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包加载任务完成回调的原型
    /// </summary>
    public delegate void LoadBundleCallback(bool success);
    
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
        private LoadBundleCallback onBuiltInShaderBundleLoadedCallback;
        protected LoadBundleCallback OnFinished;
        protected BundleRuntimeInfo BundleRuntimeInfo;
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
            onBuiltInShaderBundleLoadedCallback = OnBuiltInShaderBundleLoadedCallback;
        }

        /// <inheritdoc />
        public override void Run()
        {
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
                //不需要加载内置资源包
                loadState = LoadBundleState.BundleNotLoad;
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
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
        /// 内置Shader资源包加载完毕回调
        /// </summary>
        private void OnBuiltInShaderBundleLoadedCallback(bool success)
        {
            loadState = LoadBundleState.BuiltInShaderBundleLoaded;
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
                builtInShaderBundleRuntimeInfo.AddDownStream(BundleRuntimeInfo);
                BundleRuntimeInfo.AddUpStream(builtInShaderBundleRuntimeInfo);
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
                OnFinished?.Invoke(false);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.OnFinished?.Invoke(false);
                }
            }
            else
            {
                //Debug.Log($"资源包加载成功：{bundleRuntimeInfo.Manifest}");
                OnFinished?.Invoke(true);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.OnFinished?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            request =  AssetBundle.LoadFromFileAsync(BundleRuntimeInfo.LoadPath);
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
        public static LoadBundleTask Create(TaskRunner owner, string name,LoadBundleCallback callback)
        {
            LoadBundleTask task = ReferencePool.Get<LoadBundleTask>();
            task.CreateBase(owner,name);
            
            task.OnFinished = callback;
            task.BundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(name);
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            
            OnFinished = default;
            BundleRuntimeInfo = default;
            request = default;
            loadState = default;
        }
    }
}