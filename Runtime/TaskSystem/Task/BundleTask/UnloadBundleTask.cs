

using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包卸载的任务
    /// </summary>
    public class UnloadBundleTask : BaseTask
    {
        private BundleRuntimeInfo bundleRuntimeInfo;
        private float timer;
        
        /// <inheritdoc />
        public override float Progress => timer / CatAssetManager.UnloadBundleDelayTime;

        public override void Run()
        {

        }

        public override void Update()
        {
            if (bundleRuntimeInfo.UsingAssets.Count > 0 || bundleRuntimeInfo.DependencyLink.DownStream.Count > 0)
            {
                //被重新使用了 不进行卸载了
                State = TaskState.Finished;
                return;
            }
            
            timer += Time.deltaTime;
            if (timer < CatAssetManager.UnloadBundleDelayTime)
            {
                //状态修改为Waiting 这样不占用每帧任务处理次数
                State = TaskState.Waiting;
                return;
            }
            
            //卸载时间到了
            State = TaskState.Finished;
            
            //删除此资源包中已加载的资源实例与AssetRuntimeInfo的关联
            foreach (AssetManifestInfo assetManifestInfo in bundleRuntimeInfo.Manifest.Assets)
            {
                AssetRuntimeInfo assetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(assetManifestInfo.Name);
                if (assetRuntimeInfo.Asset != null)
                {
                    CatAssetDatabase.RemoveAssetInstance(assetRuntimeInfo.Asset);
                    assetRuntimeInfo.Asset = null;
                }
            }

            //删除此资源包在依赖链上的信息
            //并检查上游资源包的生命周期
            foreach (BundleRuntimeInfo upStreamBundle in bundleRuntimeInfo.DependencyLink.UpStream)
            {
                upStreamBundle.RemoveDownStream(bundleRuntimeInfo);
                upStreamBundle.CheckLifeCycle();
            }
            bundleRuntimeInfo.ClearUpStream();
            
            //卸载资源包
#if UNITY_2021_1_OR_NEWER
            bundleRuntimeInfo.Bundle.UnloadAsync(true);
#else
            bundleRuntimeInfo.Bundle.Unload(true);
#endif
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