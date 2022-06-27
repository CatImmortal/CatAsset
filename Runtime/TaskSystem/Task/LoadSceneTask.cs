using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 场景加载任务完成回调的原型
    /// </summary>
    public delegate void LoadSceneTaskCallback(bool success, Scene scene, object userdata);
    
    /// <summary>
    /// 场景加载任务
    /// </summary>
    public class LoadSceneTask : LoadAssetTask<Object>
    {
        private LoadSceneTaskCallback onFinished;
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
                if (!NeedCancel)
                {
                    onFinished?.Invoke(true, loadedScene, Userdata);
                }
                else
                {
                    //被取消了 卸载场景
                    CatAssetManager.UnloadScene(loadedScene);
                }
                
                //加载成功时 无论主任务是否被取消 都要重新运行未取消的已合并任务
                //因为每次加载场景都是在实例化一个新场景 不存在复用的概念
                foreach (LoadSceneTask task in mergedTasks)
                {
                    if (!task.NeedCancel)
                    {
                        CatAssetManager.LoadScene(task.Name,task.Userdata,task.onFinished);
                    }
                }
                
               
            }
            else
            {
                if (!NeedCancel)
                {
                    onFinished?.Invoke(false, default, Userdata);
                    foreach (LoadSceneTask task in mergedTasks)
                    {
                        task.onFinished?.Invoke(false, default, Userdata);
                    }
                }
              
            }
            
           
            
        }

        /// <summary>
        /// 创建场景加载任务的对象
        /// </summary>
        public static LoadSceneTask Create(TaskRunner owner, string name,object userdata,LoadSceneTaskCallback callback)
        {
            LoadSceneTask task = ReferencePool.Get<LoadSceneTask>();
            task.CreateBase(owner,name);

            task.AssetRuntimeInfo = CatAssetManager.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetManager.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.RelativePath);
            task.Userdata = userdata;
            task.onFinished = callback;

            return task;
        }

        public override void Clear()
        {
            base.Clear();

            loadedScene = default;
        }
    }
}