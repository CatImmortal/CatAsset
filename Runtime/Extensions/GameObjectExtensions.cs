using UnityEngine;

namespace CatAsset.Runtime
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 获取组件，若不存在则添加
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T: Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }

            return component;
        }
        
        /// <summary>
        /// 将资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static void Bind(this GameObject go,AssetHandler handler)
        {
            CatAssetManager.BindToGameObject(go,handler);
        }
    }
}