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
    }
}