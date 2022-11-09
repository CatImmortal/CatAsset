using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static class HandlerExtensions
    {
        /// <summary>
        /// 将资源句柄绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static T BindTo<T>(this T self, GameObject target) where T : IBindableHandler
        {
            target.BindTo(self);
            return self;
        }
        
        /// <summary>
        /// 将资源句柄绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static T BindTo<T>(this T self, Scene target) where T : IBindableHandler
        {
            target.BindTo(self);
            return self;
        }
    }
}