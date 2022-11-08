using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CatAsset.Runtime
{
    public static class UnityExtensions
    {
        /// <summary>
        /// 获取组件，若不存在则添加
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject self) where T: Component
        {
            T component = self.GetComponent<T>();
            if (component == null)
            {
                component = self.AddComponent<T>();
            }

            return component;
        }
        
        /// <summary>
        /// 将资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public static void BindTo(this GameObject self,IBindableHandler handler)
        {
            CatAssetManager.BindToGameObject(self,handler);
        }
        
        /// <summary>
        /// 将资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public static void BindTo(this Scene self,IBindableHandler handler)
        {
            CatAssetManager.BindToScene(self,handler);
        }
        
        /// <summary>
        /// 设置图片
        /// </summary>
        public static void SetSprite(this Image self, string assetName)
        {
            CatAssetManager.LoadAssetAsync<Sprite>(assetName,(handler =>
            {
                if (handler.State == HandlerState.Success)
                {
                    self.sprite = handler.Asset;
                    self.gameObject.BindTo(handler);
                }
                
            }));
        }

        /// <summary>
        /// 设置图片
        /// </summary>
        public static void SetTexture(this RawImage self, string assetName)
        {
            CatAssetManager.LoadAssetAsync<Texture>(assetName, (handler =>
            {
                if (handler.State == HandlerState.Success)
                {
                    self.texture = handler.Asset;
                    self.gameObject.BindTo(handler);
                }
            }));
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        public static void SetText(this Text self, string assetName)
        {
            CatAssetManager.LoadAssetAsync<TextAsset>(assetName, (handler =>
            {
                if (handler.State == HandlerState.Success)
                {
                    self.text = handler.Asset.text;
                    self.gameObject.BindTo(handler);
                }
            }));
        }

        /// <summary>
        /// 设置视频片段
        /// </summary>
        public static void SetVideoClip(this VideoPlayer self,string assetName)
        {
            CatAssetManager.LoadAssetAsync<VideoClip>(assetName, (handler =>
            {
                if (handler.State == HandlerState.Success)
                {
                    self.clip = handler.Asset;
                    self.gameObject.BindTo(handler);
                }
                
            }));
        }
    }
}