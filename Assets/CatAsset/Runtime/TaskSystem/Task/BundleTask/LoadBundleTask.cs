using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包加载任务完成回调的原型
    /// </summary>
    public delegate void LoadBundleCallback(bool success,object userdata);
    
    /// <summary>
    /// 资源包加载任务
    /// </summary>
    public class LoadBundleTask : BaseTask<LoadBundleTask>
    {
        /// <summary>
        /// 资源包加载状态
        /// </summary>
        private enum LoadBundleStatus
        {
            None,

            /// <summary>
            /// 资源包加载中
            /// </summary>
            Loading,

            /// <summary>
            /// 资源包加载结束
            /// </summary>
            Loaded,

        }

        private object userdata;
        private LoadBundleCallback onFinished;
        
        private BundleRuntimeInfo bundleRuntimeInfo;
        private LoadBundleStatus loadBundleState;
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
        
       
        
        /// <inheritdoc />
        public override void Run()
        {
            loadBundleState = LoadBundleStatus.Loading;
            request =  AssetBundle.LoadFromFileAsync(bundleRuntimeInfo.LoadPath);
        }

        /// <inheritdoc />
        public override void Update()
        {
            switch (loadBundleState)
            {
                case LoadBundleStatus.Loading:
                    //加载中
                    CheckStateWithLoading();
                    break;
                
                case LoadBundleStatus.Loaded:
                    //加载结束
                    CheckStateWithLoaded();
                    break;

            }
        }
        
        private void CheckStateWithLoading()
        {
            State = TaskState.Running;

            if (request.isDone)
            {
                loadBundleState = LoadBundleStatus.Loaded;
                bundleRuntimeInfo.Bundle = request.assetBundle;
            }
        }
        
        private void CheckStateWithLoaded()
        {
            State = TaskState.Finished;
            
            if (bundleRuntimeInfo.Bundle == null)
            {
                Debug.LogError($"资源包加载失败：{bundleRuntimeInfo.Manifest}");
                onFinished?.Invoke(false,userdata);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(false,task.userdata);
                }
            }
            else
            {
                //Debug.Log($"资源包加载成功：{bundleRuntimeInfo.Manifest}");
                onFinished?.Invoke(true,userdata);
                foreach (LoadBundleTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(true,task.userdata);
                }
            }
        }
        
        /// <summary>
        /// 创建资源包加载任务的对象
        /// </summary>
        public static LoadBundleTask Create(TaskRunner owner, string name,object userdata,LoadBundleCallback callback)
        {
            LoadBundleTask task = ReferencePool.Get<LoadBundleTask>();
            task.CreateBase(owner,name);
            
            task.userdata = userdata;
            task.onFinished = callback;
            task.bundleRuntimeInfo = CatAssetManager.GetBundleRuntimeInfo(name);
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            userdata = default;
            onFinished = default;
            bundleRuntimeInfo = default;
            request = default;
            loadBundleState = default;
        }
    }
}