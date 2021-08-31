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
        private List<string> assetNames;

        private bool flag;

        public LoadAssetsTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
            assetNames = (List<string>)userData;
        }

        public override void Execute()
        {
            foreach (string assetName in assetNames)
            {
                CatAssetManager.LoadAsset(assetName, null);
            }
        }

        public override void UpdateState()
        {
            if (flag == false)
            {
                //Execute执行过，要等一帧
                flag = true;
                State = TaskState.Waiting;
                return;
            }

            if (CheckLoadAssetsFinished())
            {
                State = TaskState.Finished;
                Completed?.Invoke(GetLoadedAssets());
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

