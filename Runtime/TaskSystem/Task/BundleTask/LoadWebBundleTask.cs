using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    /// <summary>
    /// WebGL资源包加载任务
    /// </summary>
    public class LoadWebBundleTask : LoadBundleTask
    {
        /// <summary>
        /// WebGL资源包加载状态
        /// </summary>
        private enum LoadWebBundleState
        {
            /// <summary>
            /// 资源包加载中
            /// </summary>
            Loading,

            /// <summary>
            /// 资源包加载结束
            /// </summary>
            Loaded,

        }
        
        private LoadBundleCallback onFinished;
        private BundleRuntimeInfo bundleRuntimeInfo;
        private LoadWebBundleState loadState;
        private UnityWebRequestAsyncOperation op;
        
        /// <inheritdoc />
        public override float Progress
        {
            get
            {
                if (op == null)
                {
                    return 0;
                }

                return op.progress;
            }
        }
        
        public override void Run()
        {
            loadState = LoadWebBundleState.Loading;
            UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleRuntimeInfo.LoadPath,Hash128.Parse(bundleRuntimeInfo.Manifest.Hash));
            op = uwr.SendWebRequest();
        }

        public override void Update()
        {
            switch (loadState)
            {
                case LoadWebBundleState.Loading:
                    //加载中
                    CheckStateWithLoading();
                    break;
                
                case LoadWebBundleState.Loaded:
                    //加载结束
                    CheckStateWithLoaded();
                    break;

            }
        }
        
        private void CheckStateWithLoading()
        {
            State = TaskState.Running;

            if (op.webRequest.isDone)
            {
                loadState = LoadWebBundleState.Loaded;

                if (op.webRequest.result == UnityWebRequest.Result.Success)
                {
                    bundleRuntimeInfo.Bundle = DownloadHandlerAssetBundle.GetContent(op.webRequest);
                }
            }
        }
        
        private void CheckStateWithLoaded()
        {
            State = TaskState.Finished;
            
            if (bundleRuntimeInfo.Bundle == null)
            {
                Debug.LogError($"WebGL资源包加载失败：{bundleRuntimeInfo.Manifest}，错误信息：{op.webRequest.error}");
                onFinished?.Invoke(false);
                foreach (LoadWebBundleTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(false);
                }
            }
            else
            {
                //Debug.Log($"资源包加载成功：{bundleRuntimeInfo.Manifest}");
                onFinished?.Invoke(true);
                foreach (LoadWebBundleTask task in MergedTasks)
                {
                    task.onFinished?.Invoke(true);
                }
            }
        }
        
        /// <summary>
        /// 创建资源包加载任务的对象
        /// </summary>
        public static LoadWebBundleTask Create(TaskRunner owner, string name,LoadBundleCallback callback)
        {
            LoadWebBundleTask task = ReferencePool.Get<LoadWebBundleTask>();
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
            loadState = default;
            op?.webRequest.Dispose();
            op = default;
        }
    }
}