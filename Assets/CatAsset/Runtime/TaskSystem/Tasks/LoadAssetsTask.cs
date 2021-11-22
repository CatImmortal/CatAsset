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


        /// <summary>
        /// 已加载的资源数量
        /// </summary>
        private int loadedAssetCount;

        /// <summary>
        /// Asset加载完毕的回调
        /// </summary>
        private Action<bool, Object> onAssetLoaded;

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
            onAssetLoaded = OnAssetLoaded;
        }

        public override void Execute()
        {
            TaskState = TaskStatus.Waiting;
            foreach (string assetName in assetNames)
            {
                CatAssetManager.LoadAsset(assetName, onAssetLoaded);
            }
        }

        public override void Update()
        {

        }

        /// <summary>
        /// Asset加载完毕的回调
        /// </summary>
        private void OnAssetLoaded(bool success,Object asset)
        {
            loadedAssetCount++;
            if (loadedAssetCount != assetNames.Count)
            {
                //还没有全部加载完毕
                TaskState = TaskStatus.Waiting;
                return;
            }

            //全部加载完毕了
            TaskState = TaskStatus.Finished;

            //要保证资源顺序和传入的名字顺序一致
            List<Object> loadedAssets = new List<Object>(assetNames.Count);
            for (int i = 0; i < assetNames.Count; i++)
            {
                string assetName = assetNames[i];
                CatAssetManager.assetInfoDict.TryGetValue(assetName, out AssetRuntimeInfo assetInfo);
                loadedAssets.Add(assetInfo.Asset);
            }

            onFinished?.Invoke(loadedAssets);
        }

    }
}

