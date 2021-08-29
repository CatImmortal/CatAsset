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

        /// <summary>
        /// 计时器
        /// </summary>
        private float timer;

        public override float Progress
        {
            get
            {
                return timer;
            }
        }

        public UnloadAssetBundleTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
            abInfo = (AssetBundleRuntimeInfo)userData;
            State = TaskState.Waiting;  //初始状态设置为Waiting 避免占用每帧任务处理次数
        }

        public override void Execute()
        {
           
        }

        public override void UpdateState()
        {
            if (abInfo.UsedAsset.Count > 0)
            {
                //被重新使用了 不进行卸载了
                State = TaskState.Done;
                return;
            }

            timer += Time.deltaTime;
            //Debug.Log("卸载AB计时：" + timer + ",AB名：" + Name);
            if (timer >= delayUnloadTime)
            {
                //时间到了 
                State = TaskState.Done;

                //解除此AssetBundle中已加载的资源与AssetRuntimeInfo的关联
                for (int i = 0; i < abInfo.ManifestInfo.Assets.Length; i++)
                {
                    string assetName = abInfo.ManifestInfo.Assets[i].AssetName;
                    AssetRuntimeInfo info = CatAssetManager.GetAssetInfo(assetName);
                    if (info.Asset != null)
                    {
                        info.UseCount = 0;  //重置引用计数
                        CatAssetManager.RemoveAssetToRuntimeInfo(info);  //删除关联
                    }
                }

                //卸载AssetBundle
                abInfo.AssetBundle.Unload(true);
                Debug.Log("已卸载AB:" + Name);

                return;
            }

            State = TaskState.Waiting;
        }
    }
}

