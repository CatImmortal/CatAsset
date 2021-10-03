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

        private Action<bool, Object> onFinished;

        internal override Delegate FinishedCallback
        {
            get
            {
                return onFinished;
            }

            set
            {
                onFinished = (Action<bool, Object>)value;
            }
        }

        public override float Progress
        {
            get
            {
                return timer / delay;
            }
        }

        public LoadEditorAssetTask(TaskExcutor owner, string name,Action<bool, Object> onFinished) : base(owner, name)
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
                onFinished?.Invoke(true, asset);
#endif
                
                return;
            }

            State = TaskState.Waiting;
        }

    
    }
}

