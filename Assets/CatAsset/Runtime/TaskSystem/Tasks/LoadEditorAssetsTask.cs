using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset
{

    /// <summary>
    /// 编辑器资源模式下批量加载Asset的任务
    /// </summary>
    public class LoadEditorAssetsTask : BaseTask
    {
        private List<string> assetNames;
        private Action<List<Object>,object> onFinished;

        private float delay;
        private float timer;

        public override float Progress
        {
            get
            {
                return timer / delay;
            }
        }

        public LoadEditorAssetsTask(TaskExcutor owner, string name, int priority, object userData, List<string> assetNames, Action<List<Object>, object> onFinished) : base(owner, name, priority,userData)
        {
            this.assetNames = assetNames;
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
                List<Object> loadedAssets = new List<Object>();
                foreach (string assetName in assetNames)
                {
                    Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
                    loadedAssets.Add(asset);
                }
                onFinished.Invoke(loadedAssets,UserData);
#endif
                return;
            }

            State = TaskState.Waiting;
        }

    }
}

