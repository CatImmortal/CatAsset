using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatAsset.Runtime
{
    public static class HandlerExtensions
    {
        /// <summary>
        /// 将资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static AssetHandler<T> BindTo<T>(this AssetHandler<T> self,GameObject target)
        {
            target.BindTo(self);
            return self;
        }
        
        /// <summary>
        /// 将资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static AssetHandler<T> BindTo<T>(this AssetHandler<T> self,Scene target)
        {
            target.BindTo(self);
            return self;
        }
        
        /// <summary>
        /// 将批量资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static BatchAssetHandler BindTo(this BatchAssetHandler self,GameObject target)
        {
            target.BindTo(self);
            return self;
        }
        
        /// <summary>
        /// 将批量资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static BatchAssetHandler BindTo(this BatchAssetHandler self,Scene target)
        {
            target.BindTo(self);
            return self;
        }
    }
}