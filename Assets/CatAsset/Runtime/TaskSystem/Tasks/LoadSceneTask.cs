using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CatAsset
{
    /// <summary>
    /// 加载场景的任务
    /// </summary>
    public class LoadSceneTask : LoadAssetTask
    {
        public LoadSceneTask(TaskExcutor owner, string name, int priority, object userData, Action<bool, Object,object> onFinished) : base(owner, name, priority, userData, onFinished)
        {
        }

        protected override void LoadAsync()
        {
            asyncOp = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
            asyncOp.priority = Priority;
        }

        protected override void LoadDone()
        {
            //场景加载完毕
            onFinished(true, null, UserData);
            return;
        }

     
    }
}

