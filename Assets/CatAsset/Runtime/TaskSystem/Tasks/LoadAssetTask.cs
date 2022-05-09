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
        /// Asset加载状态
        /// </summary>
        private enum LoadAssetStatus
        {
            None,

            /// <summary>
            /// Bundle加载中
            /// </summary>
            BundleLoading,

            /// <summary>
            /// Bundle加载结束
            /// </summary>
            BundleLoaded,

            /// <summary>
            /// 依赖Asset加载中
            /// </summary>
            DependciesLoading,

            /// <summary>
            /// 依赖Asset加载结束
            /// </summary>
            DependciesLoaded,

            /// <summary>
            /// Asset加载中
            /// </summary>
            AssetLoading,

            /// <summary>
            /// Asset加载结束
            /// </summary>
            AssetLoaded,
        }

        /// <summary>
        /// Asset加载状态
        /// </summary>
        private LoadAssetStatus loadAssetState;

        /// <summary>
        /// 总的依赖Asset数量
        /// </summary>
        private int totalDependencyCount;

        /// <summary>
        /// 已加载的依赖Asset数量
        /// </summary>
        private int loadedDependencyCount;

        private Action<bool, Object> onDependencyLoaded;

        protected AsyncOperation asyncOp;

        protected BundleRuntimeInfo bundleInfo;
        protected AssetRuntimeInfo assetInfo;
        

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
            assetInfo = CatAssetManager.assetInfoDict[Name];
            bundleInfo = CatAssetManager.bundleInfoDict[assetInfo.BundleName];
            onDependencyLoaded = OnDependencyLoaded;
            this.onFinished = onFinished;
        }


        public override void Execute()
        {
            if (bundleInfo.Bundle == null)
            {
                //Bundle未加载到内存中 加载Bundle

                loadAssetState = LoadAssetStatus.BundleLoading;
                LoadBundleTask task = new LoadBundleTask(owner, assetInfo.BundleName, OnBundleLoaded);
                owner.AddTask(task);
            }
            else
            {
                //Bundle已加载到内存中 直接转移到BundleLoaded状态
                loadAssetState = LoadAssetStatus.BundleLoaded;
            }
        }

        public override void Update()
        {
            //1.加载Bundle
            if (loadAssetState == LoadAssetStatus.BundleLoading)
            {
                TaskState = TaskStatus.Waiting;
                return;
            }

            if (loadAssetState == LoadAssetStatus.BundleLoaded)
            {

                //添加引用计数
                assetInfo.RefCount++;
                bundleInfo.UsedAssets.Add(Name);

                //加载依赖
                loadAssetState = LoadAssetStatus.DependciesLoading;

                totalDependencyCount = assetInfo.ManifestInfo.Dependencies.Length;

                foreach (string dependency in assetInfo.ManifestInfo.Dependencies)
                {
                    CatAssetManager.LoadAsset(dependency, onDependencyLoaded);
                }

                
            }

            //2.加载依赖Asset
            if (loadAssetState == LoadAssetStatus.DependciesLoading)
            {
                TaskState = TaskStatus.Waiting;

                //依赖加载中
                if (loadedDependencyCount != totalDependencyCount)
                {
                    return;
                }

                //依赖加载结束
                loadAssetState = LoadAssetStatus.DependciesLoaded;
            }


            if (loadAssetState == LoadAssetStatus.DependciesLoaded)
            {
                //依赖加载结束

                if (assetInfo.Asset == null)
                {
                    loadAssetState = LoadAssetStatus.AssetLoading;
                    LoadAsync();
                }
                else
                {
                    loadAssetState = LoadAssetStatus.AssetLoaded;
                }
            }

            //3.加载Asset
            if (loadAssetState == LoadAssetStatus.AssetLoading)
            {
                TaskState = TaskStatus.Executing;

                if (!asyncOp.isDone)
                {
                    return;
                }

                loadAssetState = LoadAssetStatus.AssetLoaded;
                LoadDone();
            }

            //4.Asset加载结束
            if (loadAssetState == LoadAssetStatus.AssetLoaded)
            {
                TaskState = TaskStatus.Finished;

                if (bundleInfo.Bundle == null || (!bundleInfo.ManifestInfo.IsScene && assetInfo.Asset == null))
                {
                    //Bundle加载失败 或者 Asset加载失败 

                    Debug.LogError("Asset加载失败：" + Name);

                    
                    if (bundleInfo.Bundle)
                    {
                        //Bundle加载成功 但是Asset加载失败

                        //清空Asset的引用计数
                        assetInfo.RefCount = 0;
                        bundleInfo.UsedAssets.Remove(Name);
                        CatAssetManager.CheckBundleLifeCycle(bundleInfo);

                        //加载过依赖 卸载依赖
                        UnloadDependencies();
                    }

                    onFinished?.Invoke(false, null);
                }
                else
                {
                    Debug.Log("Asset加载成功：" + Name);
                    onFinished?.Invoke(true, assetInfo.Asset);
                }
            }

        }

        /// <summary>
        /// Bundle加载结束的回调
        /// </summary>
        private void OnBundleLoaded(bool success)
        {
            if (!success)
            {
                //Bundle加载失败了 直接转移到AssetLoaded状态
                loadAssetState = LoadAssetStatus.AssetLoaded;
                return;
            }

            loadAssetState = LoadAssetStatus.BundleLoaded;
        }

        /// <summary>
        /// 依赖资源加载完毕的回调
        /// </summary>
        private void OnDependencyLoaded(bool success, Object asset)
        {
            loadedDependencyCount++;

            if (success)
            {
                
                AssetRuntimeInfo dependencyAssetInfo = CatAssetManager.assetToAssetInfoDict[asset];
                BundleRuntimeInfo dependencyBundleInfo = CatAssetManager.bundleInfoDict[dependencyAssetInfo.BundleName];

                if (dependencyAssetInfo.BundleName!= bundleInfo.ManifestInfo.BundleName && !bundleInfo.DependencyBundles.Contains(dependencyAssetInfo.BundleName))
                {
                    //记录依赖到的其他Bundle 增加其引用计数
                    bundleInfo.DependencyBundles.Add(dependencyAssetInfo.BundleName);
                    dependencyBundleInfo.DependencyCount++;
                }
            }

          
        }

        /// <summary>
        /// 发起异步加载
        /// </summary>
        protected virtual void LoadAsync()
        {
            asyncOp = bundleInfo.Bundle.LoadAssetAsync(Name);
        }

        /// <summary>
        /// 加载结束
        /// </summary>
        protected virtual void LoadDone()
        {
            AssetBundleRequest abAsyncOp = (AssetBundleRequest)asyncOp;
            assetInfo.Asset = abAsyncOp.asset;

            if (assetInfo.Asset)
            {
                //添加关联
                CatAssetManager.assetToAssetInfoDict[assetInfo.Asset] = assetInfo;
            }
        }

        

        /// <summary>
        /// 卸载依赖的Asset
        /// </summary>
        protected void UnloadDependencies()
        {
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependencyName = assetInfo.ManifestInfo.Dependencies[i];

                if (CatAssetManager.assetInfoDict.TryGetValue(dependencyName,out AssetRuntimeInfo dependencyInfo))
                {
                    if (dependencyInfo.Asset != null)
                    {
                        //将已加载好的依赖都卸载了
                        CatAssetManager.UnloadAsset(dependencyInfo.Asset);
                    }
                }
            }
        }



     

    



    }
}

