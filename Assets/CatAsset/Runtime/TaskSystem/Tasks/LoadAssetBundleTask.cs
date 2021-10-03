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

        public LoadAssetBundleTask(TaskExcutor owner, string name, int priority) : base(owner, name, priority)
        {
            abInfo = CatAssetManager.GetAssetBundleInfo(name);
        }

        public override void Execute()
        {
            asyncOp = AssetBundle.LoadFromFileAsync(abInfo.LoadPath);
            asyncOp.priority = Priority;
        }

        public override void UpdateState()
        {


            if (asyncOp.isDone)
            {
                State = TaskState.Finished;

                if (asyncOp.assetBundle == null)
                {
                    //AssetBundle加载失败
                    abInfo.LoadFailed = true;
                    Debug.LogError("AssetBundle加载失败：" + Name);
                    return;
                }

                //AssetBundle加载完毕
                abInfo.LoadFailed = false;
                abInfo.AssetBundle = asyncOp.assetBundle;
                return;
            }

            State = TaskState.Executing;
        }
    }
}

