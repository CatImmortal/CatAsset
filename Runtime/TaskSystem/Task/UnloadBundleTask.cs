

using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包卸载的任务
    /// </summary>
    public class UnloadBundleTask : BaseTask<UnloadBundleTask>
    {
        private BundleRuntimeInfo bundleRuntimeInfo;
        
        /// <summary>
        /// 卸载倒计时
        /// </summary>
        private float timer;
        
        /// <inheritdoc />
        public override float Progress => timer / CatAssetManager.UnloadDelayTime;

        public override void Run()
        {
            //初始状态设置为Waiting 避免占用每帧任务处理次数
            State = TaskState.Waiting;
        }

        public override void Update()
        {
            if (bundleRuntimeInfo.UsedAssets.Count > 0)
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
                    CatAssetManager.RemoveAssetRuntimeInfo(assetRuntimeInfo.Asset);
                    assetRuntimeInfo.Asset = null;
                }
            }

            //尝试卸载此资源包依赖的资源包
            foreach (BundleRuntimeInfo dependencyBundleInfo in bundleRuntimeInfo.DependencyBundles)
            {
                dependencyBundleInfo.RefBundles.Remove(bundleRuntimeInfo);
                if (dependencyBundleInfo.CanUnload())
                {
                    dependencyBundleInfo.Unload(Owner);
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
            timer = 0;
        }
    }
}