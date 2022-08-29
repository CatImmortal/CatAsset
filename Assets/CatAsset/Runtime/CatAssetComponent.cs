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

        [Header("单帧最大任务运行数量")]
        public int MaxTaskRunCount = 10;
        
        private void Awake()
        {
            CatAssetManager.RuntimeMode= RuntimeMode;
            CatAssetManager.IsEditorMode = IsEditorMode;
            CatAssetManager.UnloadDelayTime = UnloadDelayTime;
            CatAssetManager.MaxTaskRunCount = MaxTaskRunCount;
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}