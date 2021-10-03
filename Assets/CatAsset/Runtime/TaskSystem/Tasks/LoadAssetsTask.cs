using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset
{

    /// <summary>
    /// 批量加载Asset的任务
    /// </summary>
    public class LoadAssetsTask : BaseTask
    {
        private bool flag;

        private List<string> assetNames;

        private Action<List<Object>> onFinished;

        internal override Delegate FinishedCallback
        {
            get
            {
                return onFinished;
            }

            set
            {
                onFinished = (Action<List<Object>>)value;
            }
        }

        public LoadAssetsTask(TaskExcutor owner, string name,List<string> assetNames,Action<List<Object>> onFinished) : base(owner, name)
        {
            this.assetNames = assetNames;
            this.onFinished = onFinished;
        }

        public override void Execute()
        {
            flag = false;
            foreach (string assetName in assetNames)
            {
                CatAssetManager.LoadAsset(assetName, null);
            }
        }

        public override void UpdateState()
        {
            if (flag == false)
            {
                //Execute在本帧执行过的话，要等一帧,因为加载Asset的任务要到下一帧才会正式添加
                //否则CheckLoadAssetsFinished会在第一帧就返回true
                flag = true;
                State = TaskState.Waiting;
                return;
            }

            if (CheckLoadAssetsFinished())
            {
                State = TaskState.Finished;
                onFinished?.Invoke(GetLoadedAssets());
                return;
            }

            State = TaskState.Waiting;
        }

        /// <summary>
        /// 检查所有Asset的加载任务是否已结束
        /// </summary>
        private bool CheckLoadAssetsFinished()
        {
            foreach (string assetName in assetNames)
            {
                if (owner.HasTask(assetName) && owner.GetTaskState(assetName) != TaskState.Finished)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获得所有已加载的Asset
        /// </summary>
        private List<Object> GetLoadedAssets()
        {
            List<Object> loadedAssets = new List<Object>(assetNames.Count);
            foreach (string assetName in assetNames)
            {
                AssetRuntimeInfo assetInfo = CatAssetManager.GetAssetInfo(assetName);
                loadedAssets.Add(assetInfo.Asset);
            }
            return loadedAssets;
        }
    }
}

