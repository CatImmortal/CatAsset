using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset
{

    /// <summary>
    /// 加载Asset的任务
    /// </summary>
    public class LoadAssetTask : BaseTask
    {
        private AssetRuntimeInfo assetInfo;
        private AssetBundleRuntimeInfo abInfo;

        private AssetBundleRequest asyncOp;

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

        public LoadAssetTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
            assetInfo = (AssetRuntimeInfo)userData;
        }

        public override string ToString()
        {
            return Name;
        }

        public override void Execute()
        {
            abInfo = CatAssetManager.GetAssetBundleInfo(assetInfo.AssetBundleName);

            if (abInfo.AssetBundle == null)
            {
                //需要加载AssetBundle
                LoadAssetBundleTask task = new LoadAssetBundleTask(owner, abInfo.ManifestInfo.AssetBundleName, Priority + 1, null, abInfo);
                owner.AddTask(task);
            }
            else
            {
                //标记为此AssetBundle中已使用的Asset
                abInfo.UsedAsset.Add(Name);
            }

            if (!CheckDependencies())
            {
                //需要加载依赖的Asset
                for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
                {
                    string dependency = assetInfo.ManifestInfo.Dependencies[i];
                    CatAssetManager.LoadAsset(dependency, null,Priority + 1);
                }
                return;
            }
        }

        public override void Update()
        {
            if (asyncOp == null && (!CheckAssetBundle() || !CheckDependencies()))
            {
                //AssetBundle和依赖的Asset都没加载完
                //等待其他资源加载
                State = TaskState.WaitOther;
                return;
            }

            if (asyncOp == null)
            {
                //发起异步加载
                asyncOp = abInfo.AssetBundle.LoadAssetAsync(Name);
                asyncOp.priority = Priority;
            }

            if (asyncOp.isDone)
            {
                //加载完毕了
                State = TaskState.Done;
                assetInfo.Asset = asyncOp.asset;
                assetInfo.UseCount++;
                Completed?.Invoke(assetInfo.Asset);
                Debug.Log("Asset加载完毕：" + Name);
                return;
            }

            State = TaskState.Executing;
        }

        /// <summary>
        /// 检查所属的AssetBundle是否已加载好
        /// </summary>
        private bool CheckAssetBundle()
        {
            return abInfo.AssetBundle != null;

        }

        /// <summary>
        /// 检查依赖的Asset是否已加载好
        /// </summary>
        private bool CheckDependencies()
        {
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependencyName = assetInfo.ManifestInfo.Dependencies[i];

                AssetRuntimeInfo dependencyInfo = CatAssetManager.GetAssetInfo(dependencyName);
                if (dependencyInfo.Asset == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

