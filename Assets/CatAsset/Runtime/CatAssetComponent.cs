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
        public RunMode RunMode = RunMode.PackageOnly;
        public int MaxTaskExuteCount = 10;
        public int UnloadDelayTime = 5;
        public bool IsEditorMode = true;
        public float EditorModeMaxDelay = 1;


        private void Awake()
        {
            CatAssetManager.RunMode = RunMode;
          
            CatAssetManager.MaxTaskExuteCount = MaxTaskExuteCount;
            CatAssetManager.UnloadDelayTime = UnloadDelayTime;
           
            CatAssetManager.IsEditorMode = IsEditorMode;
            CatAssetManager.EditorModeMaxDelay = EditorModeMaxDelay;

        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}

