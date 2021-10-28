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
        /// <summary>
        /// 已加载的依赖数量
        /// </summary>
        private int loadedDependencyCount;

        private Action<bool, Object> onDependencyLoaded;
        private Action<bool> onAssetBundleLoaded;

        protected AsyncOperation asyncOp;

        protected AssetRuntimeInfo assetInfo;
        protected AssetBundleRuntimeInfo abInfo;

        protected Action<bool, Object> onFinished;


        internal override Delegate FinishedCallback
        {
            get
            {
                return onFinished;
            }

            set
            {
                onFinished = (Action<bool, Object>)value;
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

        public LoadAssetTask(TaskExcutor owner, string name, Action<bool, Object> onFinished) : base(owner, name)
        {
            assetInfo = CatAssetManager.GetAssetRuntimeInfo(name);
            this.onFinished = onFinished;
            onDependencyLoaded = OnDependencyLoaded;
            onAssetBundleLoaded = OnAssetBundleLoaded;
        }


        public override void Execute()
        {
            if (assetInfo.ManifestInfo.Dependencies.Length == 0)
            {
                //没有依赖需要加载 尝试加载AssetBundle
                TryLoadAssetBundle();
                return;
            }

            //加载依赖
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependency = assetInfo.ManifestInfo.Dependencies[i];
                CatAssetManager.LoadAsset(dependency,onDependencyLoaded);
            }
        }

        public override void Update()
        {

            if (asyncOp == null)
            {
                //依赖或者AssetBunlde没加载完
                State = TaskState.Waiting;
                return;
            }

            if (!asyncOp.isDone)
            {
                //加载中
                State = TaskState.Executing;
                return;
            }

            //加载完成了
            State = TaskState.Finished;
            LoadDone();
        }

        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(bool success, Object asset)
        {
            loadedDependencyCount++;

            if (loadedDependencyCount != assetInfo.ManifestInfo.Dependencies.Length)
            {
                //依赖资源未全部加载完毕
                return;
            }

            //依赖资源全部加载完毕，尝试加载AssetBundle（可能有加载失败的依赖资源，但不因此使得主资源加载失败）
            TryLoadAssetBundle();
        }

        /// <summary>
        /// 尝试加载AssetBundle
        /// </summary>
        private void TryLoadAssetBundle()
        {
            abInfo = CatAssetManager.GetAssetBundleRuntimeInfo(assetInfo.AssetBundleName);
            if (abInfo.AssetBundle == null)
            {
                //需要加载AssetBundle
                LoadAssetBundleTask task = new LoadAssetBundleTask(owner, assetInfo.AssetBundleName, onAssetBundleLoaded);
                owner.AddTask(task);
            }
            else
            {
                OnAssetBundleLoaded(true);
            }
        }

        /// <summary>
        /// AssetBundle加载完毕的回调
        /// </summary>
        private void OnAssetBundleLoaded(bool success)
        {
            if (!success )
            {
                //AssetBundle加载失败 不进行后续加载 并卸载依赖
                State = TaskState.Finished;

                assetInfo.RefCount = 0;
                UnloadDependencies();

                onFinished?.Invoke(false, null);
                return;
            }

            //进行异步加载
            LoadAsync();
            
        }

        /// <summary>
        /// 卸载依赖的Asset
        /// </summary>
        protected void UnloadDependencies()
        {
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependencyName = assetInfo.ManifestInfo.Dependencies[i];

                AssetRuntimeInfo dependencyInfo = CatAssetManager.GetAssetRuntimeInfo(dependencyName);
                if (dependencyInfo.Asset != null)
                {
                    //将已加载好的依赖都卸载了
                    CatAssetManager.UnloadAsset(dependencyInfo.Asset);
                }
            }
        }

        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            asyncOp = abInfo.AssetBundle.LoadAssetAsync(Name);
        }

        /// <summary>
        /// 加载结束
        /// </summary>
        protected virtual void LoadDone()
        {
            AssetBundleRequest abAsyncOp = (AssetBundleRequest)asyncOp;
            if (abAsyncOp.asset)
            {
                Debug.Log("Asset加载成功：" + Name);

                assetInfo.Asset = abAsyncOp.asset;

                //添加Asset和AssetRuntimeInfo的关联
                CatAssetManager.AddAssetToRuntimeInfo(assetInfo.Asset,assetInfo);  

                onFinished?.Invoke(true,assetInfo.Asset);
            }
            else
            {
                //Asset加载失败
                Debug.LogError("Asset加载失败：" + Name);

                assetInfo.RefCount = 0;

                abInfo.UsedAssets.Remove(Name);
                if (abInfo.UsedAssets.Count == 0)
                {
                    //AssetBundle此时没有Asset在使用了 创建卸载任务 开始卸载倒计时
                    UnloadAssetBundleTask task = new UnloadAssetBundleTask(owner, abInfo.ManifestInfo.AssetBundleName);
                    owner.AddTask(task);
                    Debug.Log("创建了卸载AB的任务：" + task.Name);
                }

                UnloadDependencies();

                onFinished?.Invoke(false, null);
            }
        }

    



    }
}

