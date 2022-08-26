using UnityEngine;
using CatAsset.Runtime;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源组件
    /// </summary>
    public class CatAssetComponent : MonoBehaviour
    {
        [Header("运行模式")]
        public RuntimeMode RuntimeMode = RuntimeMode.PackageOnly;
        
        [Header("编辑器资源模式")]
        public bool IsEditorMode = true;

        [Header("资源包卸载延迟")]
        public float UnloadDelayTime = 30f;

        private void Awake()
        {
            CatAssetManager.RuntimeMode= RuntimeMode;
            CatAssetManager.IsEditorMode = IsEditorMode;
            CatAssetManager.UnloadDelayTime = UnloadDelayTime;
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}