using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 卸载AssetBundle的任务
    /// </summary>
    public class UnloadAssetBundleTask : BaseTask
    {
        private AssetBundleRuntimeInfo abInfo;

        /// <summary>
        /// 延迟卸载时间
        /// </summary>
        private float delayUnloadTime = 5;

        private float timer;

        public UnloadAssetBundleTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
            abInfo = (AssetBundleRuntimeInfo)userData;
        }

        public override void Execute()
        {
            timer = 0;
        }

        public override void Update()
        {
            if (abInfo.UsedAsset.Count > 0)
            {
                State = TaskState.Done;
                return;
            }

            timer += Time.deltaTime;
            Debug.Log("卸载AB计时：" + timer + ",AB名：" + Name);
            if (timer >= delayUnloadTime)
            {
                //时间到了 卸载AssetBundle
                abInfo.AssetBundle.Unload(true);
                State = TaskState.Done;
                Debug.Log("已卸载AB:" + Name);
                return;
            }

            State = TaskState.Executing;
        }
    }
}

