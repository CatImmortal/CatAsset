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

        public LoadAssetsTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
            assetNames = (List<string>)userData;
        }

        public override void Execute()
        {
            foreach (string assetName in assetNames)
            {
                CatAssetManager.LoadAsset(assetName,null);
            }
        }

        public override void UpdateState()
        {
            if (CheckLoadAssets())
            {
                State = TaskState.Done;

                Completed?.Invoke(GetLoadedAssets());
                return;
            }

            State = TaskState.Waiting;
        }

        /// <summary>
        /// 检查Asset是否已全部加载完
        /// </summary>
        private bool CheckLoadAssets()
        {
            foreach (string assetName in assetNames)
            {
                AssetRuntimeInfo assetInfo = CatAssetManager.GetAssetInfo(assetName);
                if (assetInfo.Asset == null)
                {
                    return false;
                }
            }

            return true;
        }

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

