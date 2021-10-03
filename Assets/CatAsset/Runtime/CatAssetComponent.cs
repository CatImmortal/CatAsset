using CatJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
namespace CatAsset
{
    /// <summary>
    /// CatAsset资源组件
    /// </summary>
    public class CatAssetComponent : MonoBehaviour
    {
        public float EditorModeMaxDelay;
        public int MaxTaskExuteCount;
        public int UnloadDelayTime;
        public RunMode RunMode;
        public bool IsEditorMode;
        

        private void Awake()
        {
            CatAssetManager.EditorModeMaxDelay = EditorModeMaxDelay;
            CatAssetManager.MaxTaskExuteCount = MaxTaskExuteCount;
            CatAssetManager.UnloadDelayTime = UnloadDelayTime;
            CatAssetManager.RunMode = RunMode;
            CatAssetManager.IsEditorMode = IsEditorMode;
            
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}

