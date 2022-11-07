using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static class HandlerExtensions
    {
        /// <summary>
        /// 将资源句柄绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源句柄
        /// </summary>
        public static AssetHandler<T> BindTo<T>(this AssetHandler<T> self,GameObject target)
        {
            target.BindTo(self);
            return self;
        }
        
        /// <summary>
        /// 将资源句柄绑定到场景上，会在指定场景卸载时卸载绑定的资源句柄
        /// </summary>
        public static AssetHandler<T> BindTo<T>(this AssetHandler<T> self,Scene target)
        {
            target.BindTo(self);
            return self;
        }
        
        /// <summary>
        /// 将批量资源句柄绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源句柄
        /// </summary>
        public static BatchAssetHandler BindTo<T>(this BatchAssetHandler self,GameObject target)
        {
            foreach (AssetHandler<object> handler in self.Handlers)
            {
                target.BindTo(handler);
            }
            return self;
        }
        
        /// <summary>
        /// 将批量资源句柄绑定到场景上，会在指定场景卸载时卸载绑定的资源句柄
        /// </summary>
        public static BatchAssetHandler BindTo<T>(this BatchAssetHandler self,Scene target)
        {
            foreach (AssetHandler<object> handler in self.Handlers)
            {
                target.BindTo(handler);
            }
            return self;
        }
    }
}