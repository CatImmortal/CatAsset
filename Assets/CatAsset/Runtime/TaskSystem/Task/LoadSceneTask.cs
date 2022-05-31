using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 场景加载任务
    /// </summary>
    public class LoadSceneTask : LoadAssetTask<Object>
    {
        /// <inheritdoc />
        protected override void LoadAsync()
        {
            Operation = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
        }

        /// <inheritdoc />
        protected override void OnLoadDone()
        {
            //场景加载结束后什么都不做
        }

        /// <inheritdoc />
        protected override bool IsAssetLoadFailed()
        {
            //无法根据AssetRuntimeInfo.Asset是否为Null判断场景是否加载失败
            //只能认为是加载成功
            return false;
        }

        /// <summary>
        /// 创建场景加载任务的对象
        /// </summary>
        public static LoadSceneTask Create(TaskRunner owner, string name,object userdata,LoadAssetTaskCallback<Object> callback)
        {
            LoadSceneTask task = ReferencePool.Get<LoadSceneTask>();
            task.CreateBase(owner,name);

            task.AssetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetManager.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.RelativePath);
            task.Userdata = userdata;
            task.OnFinished = callback;
            
            return task;
        }
    }
}