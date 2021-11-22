using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 加载AssetBundle的任务
    /// </summary>
    public class LoadBundleTask : BaseTask
    {
        /// <summary>
        /// Bundle加载状态
        /// </summary>
        private enum LoadBundleStatus
        {
            None,

            /// <summary>
            /// Bundle加载中
            /// </summary>
            Loading,

            /// <summary>
            /// Bundle加载结束
            /// </summary>
            Loaded,

        }

        /// <summary>
        /// Bundle加载状态
        /// </summary>

        private LoadBundleStatus loadBundleState;



        private AssetBundleCreateRequest asyncOp;

        private BundleRuntimeInfo bundleInfo;

        private Action<bool> onFinished;

        internal override Delegate FinishedCallback
        {
            get
            {
                return onFinished;
            }

            set
            {
                onFinished = (Action<bool>)value;
            }
        }


        public override float Progress
        {
            get
            {
                if (asyncOp == null)
                {
                    return 0;
                }

                return asyncOp.progress;
            }
        }


        public LoadBundleTask(TaskExcutor owner,string name,Action<bool> onFinished) : base(owner, name)
        {
            this.onFinished = onFinished;
        }

        public override void Execute()
        {
            bundleInfo = CatAssetManager.bundleInfoDict[Name];

            loadBundleState = LoadBundleStatus.Loading;
            asyncOp = AssetBundle.LoadFromFileAsync(bundleInfo.LoadPath);
        }

        public override void Update()
        {

            if (loadBundleState == LoadBundleStatus.Loading)
            {
                //加载中
                TaskState = TaskStatus.Executing;

                if (asyncOp.isDone)
                {
                    loadBundleState = LoadBundleStatus.Loaded;

                    //TODO:Bundle解密
                    bundleInfo.Bundle = asyncOp.assetBundle;
                }

                return;
            }

            if (loadBundleState == LoadBundleStatus.Loaded)
            {
                TaskState = TaskStatus.Finished;

                //加载结束
                if (bundleInfo.Bundle == null)
                {
                    Debug.LogError("Bundle加载失败：" + Name);
                    onFinished?.Invoke(false);
                }
                else
                {
                    Debug.Log("Bundle加载成功：" + Name);
                    onFinished?.Invoke(true);
                }
            }


        }

   


    }
}

