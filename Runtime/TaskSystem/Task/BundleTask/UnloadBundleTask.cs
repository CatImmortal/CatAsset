

using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包卸载的任务
    /// </summary>
    public class UnloadBundleTask : BaseTask<UnloadBundleTask>
    {
        private BundleRuntimeInfo bundleRuntimeInfo;
        private float timer;
        
        /// <inheritdoc />
        public override float Progress => timer / CatAssetManager.UnloadDelayTime;

        public override void Run()
        {

        }

        public override void Update()
        {
            if (bundleRuntimeInfo.UsedAssets.Count > 0 || bundleRuntimeInfo.RefBundles.Count > 0)
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
            
            //删除此资源包中已加载的资源与AssetRuntimeInfo的关联
            foreach (AssetManifestInfo assetManifestInfo in bundleRuntimeInfo.Manifest.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(assetManifestInfo.Name);
                if (assetRuntimeInfo.Asset != null)
                {
                    CatAssetManager.RemoveAssetInstance(assetRuntimeInfo.Asset);
                    assetRuntimeInfo.Asset = null;
                }
            }

            //删除依赖的其他资源包的被引用记录
            foreach (BundleRuntimeInfo dependencyBundleInfo in bundleRuntimeInfo.DependencyBundles)
            {
                dependencyBundleInfo.RemoveRefBundle(bundleRuntimeInfo);
                if (dependencyBundleInfo.CanUnload())
                {
                    //依赖的其他资源包可以卸载了
                    UnloadBundleTask task = Create(Owner,dependencyBundleInfo.Manifest.RelativePath,dependencyBundleInfo);
                    Owner.AddTask(task, TaskPriority.Low);
                }
            }
            bundleRuntimeInfo.DependencyBundles.Clear();
            
            //卸载资源包
            bundleRuntimeInfo.Bundle.UnloadAsync(true);
            bundleRuntimeInfo.Bundle = null;
            
            Debug.Log($"已卸载资源包:{bundleRuntimeInfo.Manifest.RelativePath}");
        }
        
        /// <summary>
        /// 创建资源包卸载任务的对象
        /// </summary>
        public static UnloadBundleTask Create(TaskRunner owner, string name,BundleRuntimeInfo bundleRuntimeInfo)
        {
            UnloadBundleTask task = ReferencePool.Get<UnloadBundleTask>();
            task.CreateBase(owner,name);

            task.bundleRuntimeInfo = bundleRuntimeInfo;
            
            return task;
        }
        
        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            bundleRuntimeInfo = default;
            timer = default;
        }
    }
}