using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 场景加载任务完成回调的原型
    /// </summary>
    public delegate void LoadSceneCallback(bool success, Scene scene);
    
    /// <summary>
    /// 场景加载任务
    /// </summary>
    public class LoadSceneTask : LoadBundledAssetTask<object>
    {
        private LoadSceneCallback onFinished;
        private Scene loadedScene;
        
        /// <inheritdoc />
        protected override void LoadAsync()
        {
            Operation = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
        }

        /// <inheritdoc />
        protected override void LoadDone()
        {
            loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            if (loadedScene != default)
            {
                SceneManager.SetActiveScene(loadedScene);
                CatAssetDatabase.SetSceneInstance(loadedScene,AssetRuntimeInfo);
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
                    onFinished?.Invoke(true, loadedScene);
                }
                else
                {
                    //被取消了 将加载好的场景回调给下一个未被取消的已合并任务 然后将它从MergedTasks中移除掉
                    int index = -1;
                    for (int i = 0; i < MergedTaskCount; i++)
                    {
                        LoadSceneTask task = (LoadSceneTask)MergedTasks[i];
                        if (!task.NeedCancel)
                        {
                            index = i;
                            task.onFinished?.Invoke(true,loadedScene);
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        MergedTasks.RemoveAt(index);
                    }
                    else
                    {
                        //没有任何一个需要这个场景的已合并任务 直接卸载了
                        CatAssetManager.UnloadScene(loadedScene);
                    }
                }
                
                //加载成功后 无论主任务是否被取消 都要重新运行未取消的已合并任务
                //因为每次加载场景都是在实例化一个新场景 不存在复用的概念
                foreach (LoadSceneTask task in MergedTasks)
                {
                    if (!task.NeedCancel)
                    {
                        CatAssetManager.LoadSceneAsync(task.Name,task.onFinished);
                    }
                }
            }
            else
            {
                if (!NeedCancel)
                {
                    onFinished?.Invoke(false, default);
                }
                
                foreach (LoadSceneTask task in MergedTasks)
                {
                    if (!task.NeedCancel)
                    {
                        task.onFinished?.Invoke(false, default);
                    }
                       
                }
              
            }

        }

        /// <summary>
        /// 创建场景加载任务的对象
        /// </summary>
        public static LoadSceneTask Create(TaskRunner owner, string name,LoadSceneCallback callback)
        {
            LoadSceneTask task = ReferencePool.Get<LoadSceneTask>();
            task.CreateBase(owner,name);

            task.AssetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.RelativePath);
            task.onFinished = callback;

            return task;
        }

        public override void Clear()
        {
            base.Clear();
            
            onFinished = default;
            loadedScene = default;
        }
    }
}