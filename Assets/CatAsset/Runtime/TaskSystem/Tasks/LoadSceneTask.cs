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



        public LoadSceneTask(TaskExcutor owner, string name, int priority, Action<object> onCompleted, object userData) : base(owner, name, priority, onCompleted, userData)
        {
        }

        protected override void LoadAsync()
        {
            asyncOp = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
        }

        protected override void LoadDone()
        {
            //场景加载完毕
            OnCompleted?.Invoke(null);
            Debug.Log("场景加载完毕：" + Name);
            return;
        }

     
    }
}

