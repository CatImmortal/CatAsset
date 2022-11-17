using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 将资源从内存卸载的任务
    /// </summary>
    public class UnloadAssetFromMemoryTask : BaseTask
    {
        private AssetRuntimeInfo assetRuntimeInfo;
        private float timer;
        
        /// <inheritdoc />
        public override float Progress => timer / CatAssetManager.UnloadAssetDelayTime;
        
        public override void Run()
        {
            
        }

        public override void Update()
        {
            if (assetRuntimeInfo.Asset == null || assetRuntimeInfo.RefCount > 0)
            {
                //被其他地方卸载 或重新使用 不进行后续卸载处理了
                State = TaskState.Finished;
                return;
            }
            
            timer += Time.deltaTime;
            if (timer < CatAssetManager.UnloadAssetDelayTime)
            {
                //状态修改为Waiting 这样不占用每帧任务处理次数
                State = TaskState.Waiting;
                return;
            }
            
            //卸载时间到了
            State = TaskState.Finished;
            
            // BundleRuntimeInfo bundleRuntimeInfo =
            //     CatAssetDatabase.GetBundleRuntimeInfo(assetRuntimeInfo.BundleManifest.RelativePath);
            //
            // CatAssetDatabase.RemoveAssetInstance(assetRuntimeInfo.Asset);
            //
            // if (!bundleRuntimeInfo.Manifest.IsRaw)
            // {
            //     //非prefab的资源包资源 还要通过Resources.Unload卸载
            //     Object asset = (Object)assetRuntimeInfo.Asset;
            //     CatAssetManager.UnloadAssetFromMemory(asset);
            // }
            //
            // assetRuntimeInfo.Asset = null;
            
            CatAssetManager.UnloadAssetFromMemory(assetRuntimeInfo);
            
            Debug.Log($"已卸载资源:{assetRuntimeInfo}");
        }
        
        /// <summary>
        /// 创建资源包资源卸载任务的对象
        /// </summary>
        public static UnloadAssetFromMemoryTask Create(TaskRunner owner, string name,AssetRuntimeInfo assetRuntimeInfo)
        {
            UnloadAssetFromMemoryTask task = ReferencePool.Get<UnloadAssetFromMemoryTask>();
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