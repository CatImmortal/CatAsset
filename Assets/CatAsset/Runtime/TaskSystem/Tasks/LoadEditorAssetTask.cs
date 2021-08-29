using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset
{

    /// <summary>
    /// 编辑器资源模式下加载Asset的任务
    /// </summary>
    public class LoadEditorAssetTask : BaseTask
    {
        private float delay;
        private float timer;

        public override float Progress
        {
            get
            {
                return timer / delay;
            }
        }

        public LoadEditorAssetTask(TaskExcutor owner, string name, int priority, Action<object> completed, object userData) : base(owner, name, priority, completed, userData)
        {
           
        }


        public override void Execute()
        {
            //模拟异步延迟
            delay = UnityEngine.Random.Range(0, CatAssetManager.EditorModeMaxDelay);
        }

        public override void UpdateState()
        {

            timer += Time.deltaTime;
            if (timer >= delay)
            {
#if UNITY_EDITOR
                Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(Name, typeof(Object));
                Completed?.Invoke(asset);
#endif
                State = TaskState.Done;
                return;
            }

            State = TaskState.Waiting;
        }

    
    }
}

