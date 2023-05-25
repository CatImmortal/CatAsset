using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{

    /// <summary>
    /// 场景加载任务
    /// </summary>
    public class LoadSceneTask : LoadAssetTask
    {
        private SceneHandler handler;
        private Scene loadedScene;

        public override void Run()
        {
            if (AssetRuntimeInfo.RefCount > 0)
            {
                //引用计数 > 0
                //跳过依赖的加载 直接加载场景
                LoadState = LoadBundledAssetState.DependenciesLoaded;
            }
            else
            {
                base.Run();
            }
        }

        /// <inheritdoc />
        protected override void LoadAsync()
        {
            Operation = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
            Operation.priority = (int)Group.Priority;
        }

        /// <inheritdoc />
        protected override void LoadDone()
        {
            if (Operation != null)
            {
                //Operation不为null就是场景加载成功了
                loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                SceneManager.SetActiveScene(loadedScene);
                CatAssetDatabase.SetSceneInstance(loadedScene,AssetRuntimeInfo);
            }
           
        }

        /// <inheritdoc />
        protected override bool IsLoadFailed()
        {
            return Operation == null;
        }
        
        /// <inheritdoc />
        protected override void CallFinished(bool success)
        {
            if (!success)
            {
                handler.Error = "场景加载失败";
                
                //加载失败 通知所有未取消的加载任务
                foreach (LoadSceneTask task in MergedTasks)
                {
                    if (!task.IsCanceled)
                    {
                        task.handler.SetScene(default);
                    }
                    else
                    {
                        task.handler.NotifyCanceled(task.CancelToken);
                    }
                }
            }
            else
            {
                //加载成功
                AssetRuntimeInfo.AddRefCount();
                if (IsCanceled)
                {
                    //被取消了 卸载场景
                    CatAssetManager.UnloadScene(loadedScene);
                    handler.NotifyCanceled(CancelToken);
                }
                else
                {
                    handler.SetScene(loadedScene);
                }
                
                //加载成功后 无论主任务是否被取消 都要对剩余已合并任务调用InternalLoadSceneAsync重新走加载场景流程
                //因为每次加载场景都是在实例化一个新场景 无法复用
                for (int i = 1; i < MergedTasks.Count; i++)
                {
                    LoadSceneTask task = (LoadSceneTask)MergedTasks[i];
                    if (!task.IsCanceled)
                    {
                        CatAssetManager.InternalLoadSceneAsync(task.Name,task.handler,task.CancelToken,Group.Priority);
                    }
                    else
                    {
                        handler.NotifyCanceled(task.CancelToken);
                    }
                }
            }
        }

        /// <summary>
        /// 创建场景加载任务的对象
        /// </summary>
        public static LoadSceneTask Create(TaskRunner owner, string name,SceneHandler handler,CancellationToken token)
        {
            LoadSceneTask task = ReferencePool.Get<LoadSceneTask>();
            task.CreateBase(owner,name,token);

            task.handler = handler;
            task.AssetRuntimeInfo = CatAssetDatabase.GetAssetRuntimeInfo(name);
            task.BundleRuntimeInfo =
                CatAssetDatabase.GetBundleRuntimeInfo(task.AssetRuntimeInfo.BundleManifest.BundleIdentifyName);
            
            return task;
        }

        public override void Clear()
        {
            base.Clear();
            
            handler = default;
            loadedScene = default;
        }
    }
}