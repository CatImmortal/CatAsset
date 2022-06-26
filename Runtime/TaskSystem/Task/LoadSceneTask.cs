using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 场景加载任务
    /// </summary>
    public class LoadSceneTask : LoadAssetTask<Scene>
    {
        /// <summary>
        /// 已加载的场景实例
        /// </summary>
        private Scene loadedScene;
        
        /// <inheritdoc />
        protected override void LoadAsync()
        {
            Operation = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
        }

        /// <inheritdoc />
        protected override void LoadDone()
        {
            loadedScene= SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            if (loadedScene != default)
            {
                CatAssetManager.SetSceneInstance(loadedScene,AssetRuntimeInfo);
            }
        }

        /// <inheritdoc />
        protected override bool IsLoadFailed()
        {
            return loadedScene == default;
        }
        
        /// <inheritdoc />
        protected override void CallFinished(bool success)
        {
            if (success)
            {
                OnFinished?.Invoke(true, loadedScene, Userdata);
            }
            else
            {
                OnFinished?.Invoke(false, default, Userdata);
            }
            
            //对于场景资源来说 没有复用的概念 每次加载场景都需要加载新场景实例
            //所以对于已合并任务 需要在主任务结束后重新进行加载
            foreach (LoadSceneTask task in mergedTasks)
            {
                CatAssetManager.LoadScene(task.Name,task.Userdata,task.OnFinished);
            }
            
        }

        /// <summary>
        /// 创建场景加载任务的对象
        /// </summary>
        public static LoadSceneTask Create(TaskRunner owner, string name,object userdata,LoadAssetTaskCallback<Scene> callback)
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

        public override void Clear()
        {
            base.Clear();

            loadedScene = default;
        }
    }
}