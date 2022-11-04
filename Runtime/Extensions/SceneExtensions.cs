using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static class SceneExtensions
    {

        /// <summary>
        /// 将资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static void Bind(this Scene scene,IBindableHandler handler)
        {
            CatAssetManager.BindToScene(scene,handler);
        }
    }
}