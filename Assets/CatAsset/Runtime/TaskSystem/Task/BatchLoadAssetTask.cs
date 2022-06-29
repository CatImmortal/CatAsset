using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 批量资源加载任务完成回调的原型
    /// </summary>
    public delegate void BatchLoadAssetTaskCallback(List<object> assets, object userdata);
    
    /// <summary>
    /// 批量资源加载任务
    /// </summary>
    public class BatchLoadAssetTask : BaseTask<BatchLoadAssetTask>
    {
        private object userdata;
        private List<string> assetNames;
        private BatchLoadAssetTaskCallback onFinished;

        private LoadAssetTaskCallback<Object> onAssetLoadedCallback;
        private int loadedAssetCount;
        private List<object> loadedAssets;
        private List<object> loadSuccessAssets = new List<object>();
        private bool needCancel;

        public BatchLoadAssetTask()
        {
            onAssetLoadedCallback = OnAssetLoaded;
        }

       

        public override void Run()
        {
            State = TaskState.Waiting;
            foreach (string assetName in assetNames)
            {
                CatAssetManager.LoadAsset(assetName, null, onAssetLoadedCallback);
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
        private void OnAssetLoaded(bool success, Object asset, object userdata)
        {
            loadedAssetCount++;
            
            if (success)
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
                AssetRuntimeInfo assetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(assetName); 
                loadedAssets.Add(assetRuntimeInfo.Asset);
            }

            if (!needCancel)
            {
                onFinished?.Invoke(loadedAssets,userdata);
                CallMergedTasks();
            }
            else
            {
                //被取消了
                
                //只是主任务被取消了 未取消的已合并任务还需要处理
                bool called = CallMergedTasks();
                
                if (called)
                {
                    foreach (object loadSuccessAsset in loadSuccessAssets)
                    {
                        //至少有一个需要这个资源的已合并任务 那就只需要将主任务增加的那1个引用计数减去就行
                        AssetRuntimeInfo assetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(loadSuccessAsset);
                        assetRuntimeInfo.SubRefCount();
                    }
                }
                else
                {
                    //没有任何一个需要这个资源的已合并任务 直接卸载了
                    foreach (object loadSuccessAsset in loadSuccessAssets)
                    {
                        CatAssetManager.UnloadAsset(loadSuccessAsset);
                    }
                }
            }
        }

        /// <summary>
        /// 调用所有未取消的已合并任务回调
        /// </summary>
        private bool CallMergedTasks()
        {
            bool called = false;
            
            foreach (BatchLoadAssetTask task in MergedTasks)
            {
                if (!task.needCancel)
                {
                    called = true;
                    foreach (object loadSuccessAsset in loadSuccessAssets)
                    {
                        //增加已合并任务带来的引用计数
                        AssetRuntimeInfo assetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(loadSuccessAsset);
                        assetRuntimeInfo.AddRefCount();
                    }
                    task.onFinished?.Invoke(loadedAssets,task.userdata);
                }
            }

            return called;
        }
        
        /// <summary>
        /// 创建批量资源加载任务的对象
        /// </summary>
        public static BatchLoadAssetTask Create(TaskRunner owner, string name, List<string> assetNames,object userdata, 
            BatchLoadAssetTaskCallback callback)
        {
            BatchLoadAssetTask task = ReferencePool.Get<BatchLoadAssetTask>();
            task.CreateBase(owner,name);
            task.userdata = userdata;
            task.assetNames = assetNames;
            task.loadedAssets = new List<object>();
            task.onFinished = callback;
            
            return task;
        }

        public override void Clear()
        {
            base.Clear();

            assetNames = default;
            userdata = default;
            onFinished = default;

            loadedAssets = default;
            loadedAssetCount = default;
            loadSuccessAssets.Clear();
            needCancel = default;
            
        }
    }
}