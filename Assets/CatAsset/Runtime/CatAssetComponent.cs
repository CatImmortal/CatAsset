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
        public bool IsEditorMode;
        public float EditorModeMaxDelay;
      
        private void Awake()
        {
            CatAssetManager.IsEditorMode = IsEditorMode;
            CatAssetManager.EditorModeMaxDelay = EditorModeMaxDelay;
           

            

          
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}

