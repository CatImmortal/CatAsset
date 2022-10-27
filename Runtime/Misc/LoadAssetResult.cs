using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 自定义原生资源转换方法的原型
    /// </summary>
    public delegate object CustomRawAssetConverter(byte[] bytes);
    
    /// <summary>
    /// 资源加载结果
    /// </summary>
    public readonly struct LoadAssetResult
    {
        /// <summary>
        /// 已加载的原始资源实例
        /// </summary>
        public readonly object Asset;

        /// <summary>
        /// 资源类别
        /// </summary>
        public readonly AssetCategory Category;
        
        
        public LoadAssetResult(object asset, AssetCategory category)
        {
            Asset = asset;
            Category = category;
        }
        

        /// <summary>
        /// 获取已加载的指定类型资源实例
        /// </summary>
        public T GetAsset<T>()
        {
            if (Asset == null)
            {
                return default;
            }

            Type type = typeof(T);
            
            if (type == typeof(object))
            {
                return (T)Asset;
            }
            
            switch (Category)
            {
                case AssetCategory.InternalBundledAsset:
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        if (type == typeof(Sprite) && Asset is Texture2D tex)
                        {
                            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                            return (T) (object) sprite;
                        }
                        else if (type == typeof(Texture2D) && Asset is Sprite sprite)
                        {
                            return (T)(object)sprite.texture;
                        }
                        else
                        {
                            return (T) Asset;
                        }
                    }

                    Debug.LogError($"LoadAssetResult.GetAsset<T>调用失败，资源类别为{Category}，但是T为{type}");
                    return default;

                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:

                    if (type == typeof(byte[]))
                    {
                        return (T)Asset;
                    }

                    CustomRawAssetConverter converter = CatAssetManager.GetCustomRawAssetConverter(type);
                    if (converter == null)
                    {
                        Debug.LogError($"LoadAssetResult.GetAsset<T>调用失败，没有注册类型{type}的CustomRawAssetConverter");
                        return default;
                    }

                    object convertedAsset = converter((byte[])Asset);
                    return (T) convertedAsset;

            }

            
            return default;
        }
    }
}