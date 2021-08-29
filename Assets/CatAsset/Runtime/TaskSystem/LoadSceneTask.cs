using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CatAsset
{
    /// <summary>
    /// 加载场景的任务
    /// </summary>
    public class LoadSceneTask : LoadAssetTask
    {
        private AsyncOperation asyncOp;

        public override float Progress
        {
            get
            {
                if (asyncOp == null)
                {
                    return 0;
                }

                return asyncOp.progress;
            }
        }

        public LoadSceneTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
        }



        public override void UpdateState()
        {
            if (asyncOp == null && (!CheckAssetBundle() || !CheckDependencies()))
            {
                //AssetBundle或者依赖的Asset没加载完
                //等待其他资源加载
                State = TaskState.Waiting;
                return;
            }

            if (asyncOp == null)
            {
                //发起场景加载
                asyncOp = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
                asyncOp.priority = Priority;
            }

            if (asyncOp.isDone)
            {
                //场景加载完毕
                State = TaskState.Done;

                Completed?.Invoke(null);
                Debug.Log("场景加载完毕：" + Name);
                return;
            }

            State = TaskState.Executing;
        }
    }
}

