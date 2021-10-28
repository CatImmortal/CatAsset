#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset
{

    /// <summary>
    /// 编辑器资源模式下批量加载Asset的任务
    /// </summary>
    public class EditorLoadAssetsTask : BaseTask
    {
        

        private float delay;
        private float timer;
        private List<string> assetNames;
        private Action<List<Object>> onFinished;

        internal override Delegate FinishedCallback
        {
            get
            {
                return onFinished;
            }

            set
            {
                onFinished = (Action<List<Object>>)value;
            }
        }

        public override float Progress
        {
            get
            {
                return timer / delay;
            }
        }

        public EditorLoadAssetsTask(TaskExcutor owner, string name,List<string> assetNames, Action<List<Object>> onFinished) : base(owner, name)
        {
            this.assetNames = assetNames;
            this.onFinished = onFinished;
        }

        public override void Execute()
        {
            //模拟异步延迟
            delay = UnityEngine.Random.Range(0, CatAssetManager.EditorModeMaxDelay);
        }

        public override void Update()
        {
            timer += Time.deltaTime;
            if (timer >= delay)
            {
                State = TaskState.Finished;

                List<Object> loadedAssets = new List<Object>();
                foreach (string assetName in assetNames)
                {
                    Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
                    loadedAssets.Add(asset);
                }
                onFinished.Invoke(loadedAssets);

                return;
            }

            State = TaskState.Waiting;
        }

    }
}
#endif

