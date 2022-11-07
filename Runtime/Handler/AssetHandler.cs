using System;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载完毕回调方法的原型
    /// </summary>
    public delegate void AssetLoadedCallback<T>(AssetHandler<T> handler);

    /// <summary>
    /// 自定义原生资源转换方法的原型
    /// </summary>
    public delegate object CustomRawAssetConverter(byte[] bytes);

    /// <summary>
    /// 资源句柄
    /// </summary>
    public abstract class AssetHandler : BaseHandler
    {
        /// <summary>
        /// 原始资源实例
        /// </summary>
        public object AssetObj { get; protected set; }

        /// <summary>
        /// 资源类别
        /// </summary>
        public AssetCategory Category { get; protected set; }

        /// <summary>
        /// 是否加载成功
        /// </summary>
        public override bool Success => AssetObj != null;

        /// <summary>
        /// 设置原始资源实例
        /// </summary>
        internal abstract void SetAsset(object loadedAsset);

        /// <inheritdoc />
        public override void Unload()
        {
            if (!IsValid)
            {
                Debug.LogError($"卸载了无效的{GetType().Name}");
                return;
            }
            
            CatAssetManager.UnloadAsset(AssetObj);
            Release();
        }

        /// <summary>
        /// 转换原始资源实例为指定类型的资源实例
        /// </summary>
        public T AssetAs<T>()
        {
            if (AssetObj == null)
            {
                return default;
            }

            Type type = typeof(T);

            if (type == typeof(object))
            {
                return (T)AssetObj;
            }

            switch (Category)
            {
                case AssetCategory.InternalBundledAsset:
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        if (type == typeof(Sprite) && AssetObj is Texture2D tex)
                        {
                            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                            return (T) (object) sprite;
                        }
                        else if (type == typeof(Texture2D) && AssetObj is Sprite sprite)
                        {
                            return (T) (object) sprite.texture;
                        }
                        else
                        {
                            return (T)AssetObj;
                        }
                    }

                    Debug.LogError($"AssetHandler.AssetAs获取失败，资源类别为{Category}，但是T为{type}");
                    return default;

                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:

                    if (type == typeof(byte[]))
                    {
                        return (T)AssetObj;
                    }

                    CustomRawAssetConverter converter = CatAssetManager.GetCustomRawAssetConverter(type);
                    if (converter == null)
                    {
                        Debug.LogError($"AssetHandler.AssetAs获取失败，没有注册类型{type}的CustomRawAssetConverter");
                        return default;
                    }

                    object convertedAsset = converter((byte[]) AssetObj);
                    return (T) convertedAsset;

            }

            return default;
        }

        public override void Clear()
        {
            base.Clear();

            AssetObj = default;
            Category = default;
        }
    }


    /// <inheritdoc />
    public class AssetHandler<T> : AssetHandler
    {
        /// <summary>
        /// 资源实例
        /// </summary>
        public T Asset { get; private set; }

        /// <summary>
        /// 资源加载完毕回调
        /// </summary>
        private AssetLoadedCallback<T> onLoadedCallback;

        /// <inheritdoc />
        internal override void SetAsset(object loadedAsset)
        {
            AssetObj = loadedAsset;
            Asset = AssetAs<T>();
            IsDone = true;
            onLoadedCallback?.Invoke(this);
            ContinuationCallBack?.Invoke();

            if (IsValid && !Success)
            {
                //加载失败 自行释放句柄
                Release();
            }
        }

        public static AssetHandler<T> Create(AssetLoadedCallback<T> callback,AssetCategory category = AssetCategory.None)
        {
            AssetHandler<T> handler = ReferencePool.Get<AssetHandler<T>>();
            handler.IsValid = true;
            handler.Category = category;
            handler.onLoadedCallback = callback;
            return handler;
        }

        public override void Clear()
        {
            base.Clear();

            Asset = default;
            onLoadedCallback = default;
        }


    }
}
