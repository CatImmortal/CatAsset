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
        public bool IsEditorMode;
        
        private void Awake()
        {
            CatAssetManager.RuntimeMode= RuntimeMode;
            CatAssetManager.IsEditorMode = IsEditorMode;
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}