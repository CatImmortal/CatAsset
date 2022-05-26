using UnityEngine;
using CatAsset.Runtime;

namespace CatAsset.Runtime
{
    /// <summary>
    /// CatAsset资源组件
    /// </summary>
    public class CatAssetComponent : MonoBehaviour
    {
        public RuntimeMode RuntimeMode = RuntimeMode.PackageOnly;
        
        private void Awake()
        {
            CatAssetManager.RuntimeMode= RuntimeMode;
        }

        private void Update()
        {
            CatAssetManager.Update();
        }
    }
}