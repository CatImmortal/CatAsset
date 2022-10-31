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
            /// <summary>
            /// 内置Shader资源包加载中
            /// </summary>
            BuiltInShaderBundleLoading,
            
            /// <summary>
            /// 内置Shader资源包加载结束
            /// </summary>
            BuiltInShaderBundleLoaded,
            
            /// <summary>
            /// 资源包加载中
            /// </summary>
            Loading,

            /// <summary>
            /// 资源包加载结束
            /// </summary>
            Loaded,

        }
        private LoadBundleCallback onBuiltInShaderBundleLoadedCallback;
        private LoadBundleCallback onFinished;
        private BundleRuntimeInfo bundleRuntimeInfo;
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
            if (bundleRuntimeInfo.Manifest.IsDependencyBuiltInShaderBundle)
            {
                //此资源包依赖内置Shader资源包
                BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(Util.BuiltInShaderBundleName);
                if (builtInShaderBundleRuntimeInfo.Bundle == null)
                {
                    //内置Shader资源包未加载 需要加载
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
                    
                    loadState = LoadBundleState.BuiltInShaderBundleLoading;
                    return;
                }

            }

            //此资源包不依赖内置Shader资源包 或内置Shader资源包已加载 直接加载
            loadState = LoadBundleState.BuiltInShaderBundleLoaded;
           
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (loadState)
            {
                case LoadBundleState.BuiltInShaderBundleLoading:
                    //内置Shader资源包加载中
                    CheckStateWithBuiltInShaderBundleLoading();
                    break;
                
                case LoadBundleState.BuiltInShaderBundleLoaded:
                    //内置Shader资源包加载结束
                    CheckStateWithBuiltInShaderBundleLoaded();
                    break;
                
                case LoadBundleState.Loading:
                    //加载中
                    CheckStateWithLoading();
                    break;
                
                case LoadBundleState.Loaded:
                    //加载结束
                    CheckStateWithLoaded();
                    break;

            }
        }
        
        /// <summary>
        /// 内置Shader资源包加载完毕回调
        /// </summary>
        private void OnBuiltInShaderBundleLoadedCallback(bool success)
        {
            loadState = LoadBundleState.BuiltInShaderBundleLoaded;

            if (success)
            {
                //加载成功 添加依赖链记录
                BundleRuntimeInfo builtInShaderBundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(Util.BuiltInShaderBundleName);
                builtInShaderBundleRuntimeInfo.AddDownStream(bundleRuntimeInfo);
                bundleRuntimeInfo.AddUpStream(builtInShaderBundleRuntimeInfo);
            }
        }

        private void CheckStateWithBuiltInShaderBundleLoading()
        {
            State = TaskState.Waiting;
        }
        
        private void CheckStateWithBuiltInShaderBundleLoaded()
        {
            State = TaskState.Waiting;
            
            loadState = LoadBundleState.Loading;
            request =  AssetBundle.LoadFromFileAsync(bundleRuntimeInfo.LoadPath);
        }
        
        private void CheckStateWithLoading()
        {
            State = TaskState.Running;

            if (request.isDone)
            {
                loadState = LoadBundleState.Loaded;
                bundleRuntimeInfo.Bundle = request.assetBundle;
            }
        }
        
        private void CheckStateWithLoaded()
        {
            State = TaskState.Finished;
            
            if (bundleRuntimeInfo.Bundle == null)
            {
                Debug.LogError($"资源包加载失败：{bundleRuntimeInfo.Manifest}");
                onFinished?.Invoke(false);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(false);
                }
            }
            else
            {
                //Debug.Log($"资源包加载成功：{bundleRuntimeInfo.Manifest}");
                onFinished?.Invoke(true);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(true);
                }
            }
        }


        
        /// <summary>
        /// 创建资源包加载任务的对象
        /// </summary>
        public static LoadBundleTask Create(TaskRunner owner, string name,LoadBundleCallback callback)
        {
            LoadBundleTask task = ReferencePool.Get<LoadBundleTask>();
            task.CreateBase(owner,name);
            
            task.onFinished = callback;
            task.bundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(name);
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            
            onFinished = default;
            bundleRuntimeInfo = default;
            request = default;
            loadState = default;
        }
    }
}