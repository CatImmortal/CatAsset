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
        /// 计时器
        /// </summary>
        private float timer;

        public override float Progress
        {
            get
            {
                return timer / CatAssetManager.UnloadDelayTime;
            }
        }

        public UnloadAssetBundleTask(TaskExcutor owner, string name) : base(owner, name)
        {
            abInfo = CatAssetManager.GetAssetBundleRuntimeInfo(name);
           
        }

        public override void Execute()
        {
            State = TaskState.Waiting;  //初始状态设置为Waiting 避免占用每帧任务处理次数
        }

        public override void Update()
        {
            if (abInfo.UsedAssets.Count > 0)
            {
                //被重新使用了 不进行卸载了
                State = TaskState.Finished;
                return;
            }

            timer += Time.deltaTime;
            if (timer < CatAssetManager.UnloadDelayTime)
            {
                //状态修改为Waiting 这样不占用owner任务执行次数
                State = TaskState.Waiting;
                return;
            }

            //卸载时间到了 
            State = TaskState.Finished;

            //解除此AssetBundle中已加载的Asset与AssetRuntimeInfo的关联
            for (int i = 0; i < abInfo.ManifestInfo.Assets.Length; i++)
            {
                string assetName = abInfo.ManifestInfo.Assets[i].AssetName;

                AssetRuntimeInfo info = CatAssetManager.GetAssetRuntimeInfo(assetName);
                if (info.Asset != null)
                {
                    CatAssetManager.RemoveAssetToRuntimeInfo(info.Asset);  //删除关联
                    info.Asset = null;
                }
            }

            //减少依赖的AssetBundle的引用计数

            foreach (string abName in abInfo.DependencyAssetBundles)
            {
               AssetBundleRuntimeInfo dependencyABInfo = CatAssetManager.GetAssetBundleRuntimeInfo(abName);
                dependencyABInfo.RefCount--;
                if (dependencyABInfo.RefCount == 0 && dependencyABInfo.UsedAssets.Count == 0)
                {
                    //卸载依赖的AssetBundle
                    UnloadAssetBundleTask task = new UnloadAssetBundleTask(owner, dependencyABInfo.ManifestInfo.AssetBundleName);
                    owner.AddTask(task);
                    Debug.Log("创建了卸载AB的任务：" + task.Name);
                }
            }

            //卸载AssetBundle
            abInfo.AssetBundle.Unload(true);
            Debug.Log("已卸载AssetBundle:" + Name);
        }
    }
}

