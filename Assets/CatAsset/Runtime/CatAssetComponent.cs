using CatJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

            if (!IsEditorMode)
            {
                UnityWebRequest uwr = UnityWebRequest.Get(Application.streamingAssetsPath + "/CatAssetManifest.json");

                UnityWebRequestAsyncOperation result = uwr.SendWebRequest();
                result.completed += (asyncOp) =>
                {
                    string json = result.webRequest.downloadHandler.text;
                    result.webRequest.Dispose();

                    CatAssetManifest manifest = JsonParser.ParseJson<CatAssetManifest>(json);
                    CatAssetManager.CheckManifest(manifest);
                };
            }

          
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}

