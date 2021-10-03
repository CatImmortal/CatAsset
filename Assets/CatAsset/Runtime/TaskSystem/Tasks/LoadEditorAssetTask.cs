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
        private Action<bool, Object, object> onFinished;

        private float delay;
        private float timer;

        public override float Progress
        {
            get
            {
                return timer / delay;
            }
        }

        public LoadEditorAssetTask(TaskExcutor owner, string name, int priority ,object userData, Action<bool, Object, object> onFinished) : base(owner, name, priority,userData)
        {
            this.onFinished = onFinished;
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
                State = TaskState.Finished;
#if UNITY_EDITOR
                Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(Name, typeof(Object));
                onFinished?.Invoke(true, asset,UserData);
#endif
                
                return;
            }

            State = TaskState.Waiting;
        }

    
    }
}

