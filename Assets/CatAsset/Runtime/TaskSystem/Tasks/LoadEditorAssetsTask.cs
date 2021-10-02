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
        private float delay;
        private float timer;

        public override float Progress
        {
            get
            {
                return timer / delay;
            }
        }

        public LoadEditorAssetsTask(TaskExcutor owner, string name, int priority, Action<object> onCompleted, object userData) : base(owner, name, priority, onCompleted, userData)
        {
            assetNames = (List<string>)userData;
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
                List<Object> loadedAssets = new List<Object>();
                foreach (string assetName in assetNames)
                {
                    Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
                    loadedAssets.Add(asset);
                }
                OnCompleted?.Invoke(loadedAssets);
#endif
                State = TaskState.Finished;
                return;
            }

            State = TaskState.Waiting;
        }

    }
}

