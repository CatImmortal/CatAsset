using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载完毕回调方法的原型
    /// </summary>
    public delegate void AssetLoadedCallback<T>(AssetHandler<T> handler);
    
    /// <summary>
    /// 资源句柄
    /// </summary>
    public class AssetHandler<T> : BaseAssetHandler, IReference , IDisposable
    {

        /// <summary>
        /// 原始资源实例
        /// </summary>
        public object AssetObj { get; private set; }

        /// <summary>
        /// 资源实例
        /// </summary>
        public T Asset { get; private set; }

        /// <summary>
        /// 资源类别
        /// </summary>
        public AssetCategory Category { get; private set; }
        
        /// <summary>
        /// 持有此句柄的任务
        /// </summary>
        internal BaseTask Task;

        /// <summary>
        /// 是否已加载完毕
        /// </summary>
        public bool IsDone { get; private set; }
        
        /// <summary>
        /// 进度
        /// </summary>
        public float Progress => Task?.Progress ?? 0;
        
        /// <summary>
        /// 加载完毕回调
        /// </summary>
        private AssetLoadedCallback<T> onLoaded;

        /// <summary>
        /// 加载完毕回调
        /// </summary>
        public event AssetLoadedCallback<T> OnLoaded
        {
            add
            {
                if (IsDone)
                {
                    onLoaded?.Invoke(this);
                }
                else
                {
                    onLoaded += value;
                }
            }
            remove => onLoaded -= value;
        }

        /// <summary>
        /// 是否已被释放
        /// </summary>
        private bool disposed;
        
        /// <summary>
        /// 设置原始资源实例
        /// </summary>
        internal override void SetAssetObj(object assetObj)
        {
            AssetObj = assetObj;
            Asset = ConvertAsset();
            IsDone = true;
            onLoaded?.Invoke(this);
        }

        /// <summary>
        /// 转换原始资源实例为T类型的资源实例
        /// </summary>
        /// <returns></returns>
        private T ConvertAsset()
        {
            if (AssetObj == null)
            {
                return default;
            }

            Type type = typeof(T);

            if (type == typeof(object))
            {
                return Asset;
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
                            return (T) (object) sprite.texture;
                        }
                        else
                        {
                            return Asset;
                        }
                    }

                    Debug.LogError($"AssetHandler.Asset获取失败，资源类别为{Category}，但是T为{type}");
                    return default;

                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:

                    if (type == typeof(byte[]))
                    {
                        return Asset;
                    }

                    CustomRawAssetConverter converter = CatAssetManager.GetCustomRawAssetConverter(type);
                    if (converter == null)
                    {
                        Debug.LogError($"AssetHandler.Asset获取失败，没有注册类型{type}的CustomRawAssetConverter");
                        return default;
                    }

                    object convertedAsset = converter((byte[]) AssetObj);
                    return (T) convertedAsset;

            }

            return default;
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel()
        {
            Task?.Cancel();
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        private void Unload()
        {
            CatAssetManager.UnloadAsset(AssetObj);
        }
        
        /// <summary>
        /// 释放此句柄
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            
            Unload();
            ReferencePool.Release(this);
        }

        
        public static AssetHandler<T> Create(AssetCategory category = default)
        {
            AssetHandler<T> handler = ReferencePool.Get<AssetHandler<T>>();
            
            handler.Category = category;
            
            return handler;
        }

        public void Clear()
        {
            AssetObj = default;
            Asset = default;
            Category = default;
            Task = default;
            IsDone = default;
            onLoaded = default;
            disposed = default;
        }


    }
}