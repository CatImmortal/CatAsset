using CatJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
namespace CatAsset
{
    public class CatAssetComponent : MonoBehaviour
    {
        public RunMode RunMode;
        public bool IsEditorMode;
        public float EditorModeMaxDelay;
        public int MaxTaskExuteCount;

        private void Awake()
        {
            CatAssetManager.RunMode = RunMode;
            CatAssetManager.IsEditorMode = IsEditorMode;
            CatAssetManager.EditorModeMaxDelay = EditorModeMaxDelay;
            CatAssetManager.SetMaxTaskExuteCount(MaxTaskExuteCount);
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}

