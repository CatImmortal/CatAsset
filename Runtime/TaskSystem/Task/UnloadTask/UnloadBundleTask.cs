

using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源包卸载任务
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
            if (bundleRuntimeInfo.Bundle == null)
            {
                //被其他地方卸载了 不进行后续处理了
                State = TaskState.Finished;
                return;
            }
            
            if (bundleRuntimeInfo.ReferencingAssets.Count > 0 || bundleRuntimeInfo.DependencyChain.DownStream.Count > 0)
            {
                //被重新使用了 不进行后续处理了
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

            CatAssetManager.UnloadBundle(bundleRuntimeInfo);
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
