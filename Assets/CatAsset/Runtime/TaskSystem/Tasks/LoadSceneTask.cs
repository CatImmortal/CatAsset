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
        public LoadSceneTask(TaskExcutor owner, string name, Action<bool, Object> onFinished) : base(owner, name, onFinished)
        {
        }

        protected override void LoadAsync()
        {
            asyncOp = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Additive);
        }

        protected override void LoadDone()
        {
            //场景加载完毕
            onFinished(true, null);
            return;
        }

     
    }
}

