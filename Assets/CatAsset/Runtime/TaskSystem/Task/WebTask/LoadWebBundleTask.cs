using System.Threading;
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

        public override void OnPriorityChanged()
        {
            if (op != null)
            {
                op.priority = (int)Group.Priority;
            }
        }

        /// <inheritdoc />
        public override void Run()
        {
            base.Run();

            //WebGL平台 不下载远端资源包
            LoadState = LoadBundleState.BundleNotLoad;
        }

        /// <inheritdoc />
        protected override void LoadAsync()
        {
            UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(BundleRuntimeInfo.LoadPath,
                Hash128.Parse(BundleRuntimeInfo.Manifest.Hash));
            op = uwr.SendWebRequest();
            op.priority = (int)Group.Priority;
        }

        /// <inheritdoc />
        protected override bool IsLoadDone()
        {
            return op.webRequest.isDone;
        }

        /// <inheritdoc />
        protected override void LoadDone()
        {
            if (!RuntimeUtil.HasWebRequestError(op.webRequest))
            {
                BundleRuntimeInfo.Bundle = DownloadHandlerAssetBundle.GetContent(op.webRequest);
            }
        }

        /// <summary>
        /// 创建WebGL资源包加载任务的对象
        /// </summary>
        public new static LoadWebBundleTask Create(TaskRunner owner, string name, BundleManifestInfo info,
            BundleLoadedCallback callback)
        {
            LoadWebBundleTask task = ReferencePool.Get<LoadWebBundleTask>();
            task.CreateBase(owner, name);

            task.OnFinishedCallback = callback;
            task.BundleRuntimeInfo = CatAssetDatabase.GetBundleRuntimeInfo(info.BundleIdentifyName);

            return task;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            op = default;
        }
    }
}