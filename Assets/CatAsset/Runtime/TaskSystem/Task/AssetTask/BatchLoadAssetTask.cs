using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 批量资源加载任务完成回调的原型
    /// </summary>
    public delegate void BatchLoadAssetCallback(List<LoadAssetResult> assets);
    
    /// <summary>
    /// 批量资源加载任务
    /// </summary>
    public class BatchLoadAssetTask : BaseTask
    {
        private List<string> assetNames;
        private BatchLoadAssetCallback onFinished;

        private LoadAssetCallback<object> onAssetLoadedCallback;
        private int loadedAssetCount;
        private List<LoadAssetResult> loadedAssets;
        private List<object> loadSuccessAssets = new List<object>();
        
        private bool needCancel;

        /// <inheritdoc />
        public override float Progress => (loadedAssetCount * 1.0f) / assetNames.Count;

        public BatchLoadAssetTask()
        {
            onAssetLoadedCallback = OnAssetLoaded;
        }

       

        public override void Run()
        {
            State = TaskState.Waiting;
            foreach (string assetName in assetNames)
            {
                CatAssetManager.LoadAssetAsync(assetName, onAssetLoadedCallback);
            }
        }

        public override void Update()
        {
            
        }

        public override void Cancel()
        {
            needCancel = true;
        }
        
        /// <summary>
        /// 资源加载结束的回调
        /// </summary>
        private void OnAssetLoaded(object asset,LoadAssetResult result)
        {
            loadedAssetCount++;
            if (asset != null)
            {
                loadSuccessAssets.Add(asset);
            }
            
            if (loadedAssetCount != assetNames.Count)
            {
                //资源还未加载完
                State = TaskState.Waiting;
                return;
            }
            
            //全部都加载完了
            State = TaskState.Finished;
            
            //保证资源顺序和加载顺序一致
            foreach (string assetName in assetNames)
            {
                AssetCategory category = RuntimeUtil.GetAssetCategory(assetName);
                AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetName);
                loadedAssets.Add(new LoadAssetResult(assetRuntimeInfo.Asset, category));
            }

            //无需处理已合并任务 因为按照现在的设计 批量加载任务，就算是资源名列表相同，也不会判断为重复任务的
            if (!needCancel)
            {
                onFinished?.Invoke(loadedAssets);
            }
            else
            {
                //被取消了 卸载加载成功的资源
                foreach (object loadSuccessAsset in loadSuccessAssets)
                {
                    CatAssetManager.UnloadAsset(loadSuccessAsset);
                }
            }
        }



        /// <summary>
        /// 创建批量资源加载任务的对象
        /// </summary>
        public static BatchLoadAssetTask Create(TaskRunner owner, string name, List<string> assetNames,
            BatchLoadAssetCallback callback)
        {
            BatchLoadAssetTask task = ReferencePool.Get<BatchLoadAssetTask>();
            task.CreateBase(owner, name);
            task.assetNames = assetNames;
            task.loadedAssets = new List<LoadAssetResult>();
            task.onFinished = callback;

            return task;
        }

        public override void Clear()
        {
            base.Clear();

            assetNames = default;
            onFinished = default;

            loadedAssets = default;
            loadedAssetCount = default;
            loadSuccessAssets.Clear();
            
            needCancel = default;
            
        }
    }
}