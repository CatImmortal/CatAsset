﻿using System;
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
        protected AssetRuntimeInfo assetInfo;
        protected AssetBundleRuntimeInfo abInfo;

        protected AsyncOperation asyncOp;

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


        public override void Execute()
        {
            abInfo = CatAssetManager.GetAssetBundleInfo(assetInfo.AssetBundleName);

            if (abInfo.AssetBundle == null)
            {
                //需要加载AssetBundle
                LoadAssetBundleTask task = new LoadAssetBundleTask(owner, abInfo.ManifestInfo.AssetBundleName, Priority + 1, null, abInfo);
                owner.AddTask(task);
            }
        }

        public override void UpdateState()
        {

            if (asyncOp == null)
            {
               
                if (!CheckDependenciesFinished())
                {
                    //依赖资源的加载任务未全部结束
                    State = TaskState.Waiting;
                    return;
                }

                if (!CheckAssetBundleLoaded())
                {
                    //AssetBundle未加载好

                    if (abInfo.IsLoadFailed)
                    {
                        //AssetBundle加载失败 不进行后续加载 并卸载依赖
                        State = TaskState.Finished;
                        assetInfo.UseCount = 0;
                        abInfo.UsedAsset.Clear();
                        UnloadDependencies();
                        return;
                    }
                    else
                    {
                        //AssetBundle加载中...
                        State = TaskState.Waiting;
                        return;
                    }
                }

                State = TaskState.Executing;

                //发起异步加载
                LoadAsync();
                asyncOp.priority = Priority;
            }


            if (asyncOp.isDone)
            {
                //加载完成了
                State = TaskState.Finished;
                LoadDone();
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
                assetInfo.Asset = abAsyncOp.asset;
                CatAssetManager.AddAssetToRuntimeInfo(assetInfo);  //添加Asset和AssetRuntimeInfo的关联
                Completed?.Invoke(assetInfo.Asset);

                Debug.Log("Asset加载完毕：" + Name);
            }
            else
            {
                //Asset加载失败
                assetInfo.UseCount = 0;
                abInfo.UsedAsset.Remove(Name);
                UnloadDependencies();
            }
        }

        /// <summary>
        /// 检查所属的AssetBundle是否已加载好
        /// </summary>
        protected bool CheckAssetBundleLoaded()
        {
            return abInfo.AssetBundle != null;
        }

        /// <summary>
        /// 检查依赖的Asset的加载任务是否已结束
        /// </summary>
        protected bool CheckDependenciesFinished()
        {
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependencyName = assetInfo.ManifestInfo.Dependencies[i];

                if (owner.HasTask(dependencyName) && owner.GetTaskState(dependencyName) != TaskState.Finished)
                {
                    return false;
                }
            }

            return true;
        }
    
        /// <summary>
        /// 卸载依赖的Asset
        /// </summary>
        protected void UnloadDependencies()
        {
            for (int i = 0; i < assetInfo.ManifestInfo.Dependencies.Length; i++)
            {
                string dependencyName = assetInfo.ManifestInfo.Dependencies[i];

                AssetRuntimeInfo dependencyInfo = CatAssetManager.GetAssetInfo(dependencyName);
                if (dependencyInfo.Asset != null)
                {
                    //将已加载好的依赖都卸载了
                    CatAssetManager.UnloadAsset(dependencyInfo.Asset);
                }
            }
        }
    }
}

