using System;
using UnityEngine;


namespace CatAsset.Runtime
{
    /// <summary>
    /// 原生资源卸载的任务
    /// </summary>
    public class UnloadRawAssetTask : BaseTask<UnloadRawAssetTask>
    {
        private AssetRuntimeInfo assetRuntimeInfo;
        private float timer;
        
        /// <inheritdoc />
        public override float Progress => timer / CatAssetManager.UnloadDelayTime;
        
        /// <inheritdoc />
        public override void Run()
        {

        }

        /// <inheritdoc />
        public override void Update()
        {
            if (assetRuntimeInfo.UseCount > 0)
            {
                //被重新使用了 不进行卸载了
                State = TaskState.Finished;
                return;
            }
            
            timer += Time.deltaTime;
            if (timer < CatAssetManager.UnloadDelayTime)
            {
                //状态修改为Waiting 这样不占用每帧任务处理次数
                State = TaskState.Waiting;
                return;
            }
            
            //卸载时间到了
            State = TaskState.Finished;
            
            CatAssetDatabase
                .RemoveAssetInstance(assetRuntimeInfo.Asset);
            assetRuntimeInfo.Asset = null;

            Debug.Log($"已卸载原生资源:{assetRuntimeInfo}");
        }
        
        /// <summary>
        /// 创建原生资源卸载任务的对象
        /// </summary>
        public static UnloadRawAssetTask Create(TaskRunner owner, string name,AssetRuntimeInfo assetRuntimeInfo)
        {
            UnloadRawAssetTask task = ReferencePool.Get<UnloadRawAssetTask>();
            task.CreateBase(owner,name);

            task.assetRuntimeInfo = assetRuntimeInfo;
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            assetRuntimeInfo = default;
            timer = default;
        }
    }
}