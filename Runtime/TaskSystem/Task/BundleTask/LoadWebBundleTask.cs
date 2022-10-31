using UnityEngine;
using UnityEngine.Networking;

namespace CatAsset.Runtime
{
    /// <summary>
    /// WebGL资源包加载任务
    /// </summary>
    public class LoadWebBundleTask : LoadBundleTask
    {
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
        
        /// <inheritdoc />
        protected override void LoadAsync()
        {
            UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(BundleRuntimeInfo.LoadPath,Hash128.Parse(BundleRuntimeInfo.Manifest.Hash));
            op = uwr.SendWebRequest();
        }

        /// <inheritdoc />
        protected override bool IsLoadDone()
        {
            return op.webRequest.isDone;
        }

        /// <inheritdoc />
        protected override void LoadDone()
        {
            if (op.webRequest.result == UnityWebRequest.Result.Success)
            {
                BundleRuntimeInfo.Bundle = DownloadHandlerAssetBundle.GetContent(op.webRequest);
            }
        }

        /// <summary>
        /// 创建WebGL资源包加载任务的对象
        /// </summary>
        public new static LoadWebBundleTask Create(TaskRunner owner, string name,LoadBundleCallback callback)
        {
            LoadWebBundleTask task = ReferencePool.Get<LoadWebBundleTask>();
            task.CreateBase(owner,name);
            
            task.OnFinished = callback;
            task.BundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(name);
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();
            
            op?.webRequest.Dispose();
            op = default;
        }
    }
}