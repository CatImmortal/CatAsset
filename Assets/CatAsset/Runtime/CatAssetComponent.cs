using System;
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
        public float UnloadBundleDelayTime = 120f;

        [Header("资源卸载延迟")]
        public float UnloadAssetDelayTime = 60f;

        [Header("每帧最大任务运行数量")]
        public int MaxTaskRunCount = 30;

        private void Awake()
        {
            CatAssetManager.UnloadBundleDelayTime = UnloadBundleDelayTime;
            CatAssetManager.UnloadAssetDelayTime = UnloadAssetDelayTime;
            CatAssetManager.MaxTaskRunCount = MaxTaskRunCount;

            //添加调试分析器组件
            gameObject.AddComponent<ProfilerComponent>();

#if UNITY_EDITOR
            if (IsEditorMode)
            {
                CatAssetManager.SetAssetLoader<EditorAssetLoader>();
                return;
            }
#endif
            switch (RuntimeMode)
            {
                case RuntimeMode.PackageOnly:
                    CatAssetManager.SetAssetLoader<PackageOnlyAssetLoader>();
                    break;
                case RuntimeMode.Updatable:
                    CatAssetManager.SetAssetLoader<UpdatableAssetLoader>();
                    break;
            }
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}
