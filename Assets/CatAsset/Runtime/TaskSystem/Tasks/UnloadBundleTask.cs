using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset
{
    /// <summary>
    /// 卸载AssetBundle的任务
    /// </summary>
    public class UnloadBundleTask : BaseTask
    {
        private BundleRuntimeInfo bundleInfo;

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

        public UnloadBundleTask(TaskExcutor owner, string name) : base(owner, name)
        {
            bundleInfo = CatAssetManager.bundleInfoDict[name];
           
        }

        public override void Execute()
        {
            TaskState = TaskStatus.Waiting;  //初始状态设置为Waiting 避免占用每帧任务处理次数
        }

        public override void Update()
        {
            if (bundleInfo.UsedAssets.Count > 0 || bundleInfo.DependencyCount > 0)
            {
                //被重新使用了 不进行卸载了
                TaskState = TaskStatus.Finished;
                return;
            }

            timer += Time.deltaTime;
            if (timer < CatAssetManager.UnloadDelayTime)
            {
                //状态修改为Waiting 这样不占用owner任务执行次数
                TaskState = TaskStatus.Waiting;
                return;
            }

            //卸载时间到了 
            TaskState = TaskStatus.Finished;

            //解除此Bundle中已加载的Asset与AssetRuntimeInfo的关联
            for (int i = 0; i < bundleInfo.ManifestInfo.Assets.Length; i++)
            {
                string assetName = bundleInfo.ManifestInfo.Assets[i].AssetName;

                if (CatAssetManager.assetInfoDict.TryGetValue(assetName,out AssetRuntimeInfo assetInfo))
                {
                    if (assetInfo.Asset != null)
                    {
                        CatAssetManager.assetToAssetInfoDict.Remove(assetInfo.Asset);
                        assetInfo.Asset = null;
                    }
                }
            }

            //减少依赖到的Bundle的引用计数
            foreach (string bundleName in bundleInfo.DependencyBundles)
            {
                BundleRuntimeInfo dependencyBundleInfo = CatAssetManager.bundleInfoDict[bundleName];
                dependencyBundleInfo.DependencyCount--;
                CatAssetManager.CheckBundleLifeCycle(dependencyBundleInfo);
            }

            //清空依赖记录
            bundleInfo.DependencyBundles.Clear();

            //卸载Bundle
            bundleInfo.Bundle.Unload(true);
            bundleInfo.Bundle = null;

            Debug.Log("已卸载Bundle:" + Name);
        }
    }
}

