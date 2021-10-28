#if UNITY_EDITOR

using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CatAsset
{

    /// <summary>
    /// 编辑器资源模式下加载Asset的任务
    /// </summary>
    public class EditorLoadAssetTask : BaseTask
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

        public EditorLoadAssetTask(TaskExcutor owner, string name,Action<bool, Object> onFinished) : base(owner, name)
        {
            this.onFinished = onFinished;
        }


        public override void Execute()
        {
            //模拟异步延迟
            delay = Random.Range(0, CatAssetManager.EditorModeMaxDelay);
        }

        public override void Update()
        {

            timer += Time.deltaTime;
            if (timer >= delay)
            {
                State = TaskState.Finished;

                Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(Name, typeof(Object));
                onFinished?.Invoke(true, asset);

                
                return;
            }

            State = TaskState.Waiting;
        }

    
    }
}

#endif

