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
    public struct LoadAssetResult
    {
        /// <summary>
        /// 已加载的原始资源实例
        /// </summary>
        private object asset;

        /// <summary>
        /// 资源类别
        /// </summary>
        public AssetCategory Category { get; }

        public LoadAssetResult(object asset, AssetCategory category)
        {
            this.asset = asset;
            Category = category;
        }

        /// <summary>
        /// 获取已加载的原始资源实例
        /// </summary>
        public object GetAsset()
        {
            return asset;
        }

        /// <summary>
        /// 获取已加载的指定类型资源实例
        /// </summary>
        public T GetAsset<T>()
        {
            if (asset == null)
            {
                return default;
            }

            Type type = typeof(T);
            
            if (type == typeof(object))
            {
                return (T)asset;
            }
            
            switch (Category)
            {
                case AssetCategory.InternalBundleAsset:
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        return (T) asset;
                    }
                    else
                    {
                        Debug.LogError($"LoadAssetResult.GetAsset<T>调用失败，资源类别为{Category}，但是T为{type}");
                        return default;
                    }
                
                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:

                    if (type == typeof(byte[]))
                    {
                        return (T)asset;
                    }

                    CustomRawAssetConverter converter = CatAssetManager.GetCustomRawAssetConverter(type);
                    if (converter == null)
                    {
                        Debug.LogError($"LoadAssetResult.GetAsset<T>调用失败，没有注册类型{type}的CustomRawAssetConverter");
                        return default;
                    }

                    object convertedAsset = converter((byte[])asset);
                    return (T) convertedAsset;

            }

            
            return default;
        }
    }
}