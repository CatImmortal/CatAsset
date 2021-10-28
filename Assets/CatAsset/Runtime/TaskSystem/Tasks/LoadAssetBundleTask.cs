using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 加载AssetBundle的任务
    /// </summary>
    public class LoadAssetBundleTask : BaseTask
    {
        private AssetBundleCreateRequest asyncOp;

        private AssetBundleRuntimeInfo abInfo;

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


        public LoadAssetBundleTask(TaskExcutor owner, string name,Action<bool> onFinished) : base(owner, name)
        {
            abInfo = CatAssetManager.GetAssetBundleRuntimeInfo(name);
            this.onFinished = onFinished;
        }

        public override void Execute()
        {
            asyncOp = AssetBundle.LoadFromFileAsync(abInfo.LoadPath);
        }

        public override void Update()
        {
            if (!asyncOp.isDone)
            {
                //AssetBundle加载中
                State = TaskState.Executing;
                return;
            }

            State = TaskState.Finished;

            if (asyncOp.assetBundle == null)
            {
                //AssetBundle加载失败
                Debug.LogError("AssetBundle加载失败：" + Name);
                abInfo.UsedAssets.Clear();
                onFinished?.Invoke(false);
                return;
            }

            //AssetBundle加载完毕
            Debug.Log("AssetBundle加载成功：" + Name);
            abInfo.AssetBundle = asyncOp.assetBundle;
            onFinished?.Invoke(true);

        }
    }
}

