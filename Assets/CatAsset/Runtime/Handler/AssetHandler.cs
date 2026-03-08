using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 资源加载完毕回调方法的原型
    /// </summary>
    public delegate void AssetLoadedCallback<T>(AssetHandler<T> handler);

    /// <summary>
    /// 资源转换完毕回调方法的原型
    /// </summary>
    public delegate void AssetConvertedCallback<T>(T asset);

    /// <summary>
    /// 资源句柄
    /// </summary>
    public abstract class AssetHandler : BaseHandler , IBindableHandler
    {
        /// <summary>
        /// 原始资源对象
        /// </summary>
        public object AssetObj { get; protected set; }

        /// <summary>
        /// 资源类别
        /// </summary>
        public AssetCategory Category { get; protected set; }

        /// <summary>
        /// 设置原始资源对象
        /// </summary>
        internal abstract void SetAsset(object loadedAsset);

        /// <inheritdoc />
        protected override void InternalUnload()
        {
            CatAssetManager.UnloadAsset(AssetObj);
            Release();
        }

        /// <summary>
        /// 转换原始资源对象为指定类型的资源对象
        /// </summary>
        public void AssetAs<T>(AssetConvertedCallback<T> onCompleted)
        {
            if (onCompleted == null)
            {
                Debug.LogError("AssetAs的onCompleted回调不应该为null");
                return;
            }

            if (AssetObj == null)
            {
                onCompleted.Invoke(default);
                return;
            }

            Type type = typeof(T);

            if (type == typeof(object))
            {
                onCompleted.Invoke((T)AssetObj);
                return;
            }

            switch (Category)
            {
                case AssetCategory.InternalBundledAsset:
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        if (type == typeof(Sprite) && AssetObj is Texture2D tex)
                        {
                            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f,0.5f));
                            onCompleted.Invoke((T) (object) sprite);
                        }
                        else if (type == typeof(Texture2D) && AssetObj is Sprite sprite)
                        {
                            onCompleted.Invoke((T) (object) sprite.texture);
                        }
                        else
                        {
                            onCompleted.Invoke((T)AssetObj);
                        }

                        return;
                    }

                    Debug.LogError($"AssetHandler.AssetAs获取失败，资源类别为{Category}，但是T为{type}");
                    onCompleted.Invoke(default);
                    return;

                case AssetCategory.InternalRawAsset:
                case AssetCategory.ExternalRawAsset:

                    if (type == typeof(byte[]))
                    {
                        onCompleted.Invoke((T)AssetObj);
                        return;
                    }

                    ICustomRawAssetConverter converter = CatAssetManager.GetCustomRawAssetConverter(type);
                    if (converter == null)
                    {
                        Debug.LogError($"AssetHandler.AssetAs获取失败，没有注册类型{type}的CustomRawAssetConverter");
                        onCompleted.Invoke(default);
                        return;
                    }

                    var task = converter.Convert((byte[]) AssetObj);
                    if (task.IsCompleted)
                    {
                        HandleCustomRawAssetConvertTask(task);
                    }
                    else
                    {
                        task.ContinueWith(HandleCustomRawAssetConvertTask, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    return;
            }

            void HandleCustomRawAssetConvertTask(Task<object> task)
            {
                if (task.IsCompletedSuccessfully)
                {
                    onCompleted.Invoke((T) task.Result);
                }
                else
                {
                    onCompleted.Invoke(default);
                }
            }

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

        /// <summary>
        /// 资源加载完毕回调
        /// </summary>
        public event AssetLoadedCallback<T> OnLoaded
        {
            add
            {
                if (!IsValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上添加了OnLoaded回调");
                    return;
                }

                if (IsDone)
                {
                    value?.Invoke(this);
                    return;
                }

                onLoadedCallback += value;
            }

            remove
            {
                if (!IsValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上移除了OnLoaded回调");
                    return;
                }

                onLoadedCallback -= value;
            }
        }
        
        /// <inheritdoc />
        internal override void SetAsset(object loadedAsset)
        {
            Task = null;
            AssetObj = loadedAsset;
            State = AssetObj != null ? HandlerState.Success : HandlerState.Failed;

            AssetAs<T>(asset =>
            {
                Asset = asset;
                CheckError();
                onLoadedCallback?.Invoke(this);
                AsyncStateMachineMoveNext?.Invoke(); 
            });
        }

        /// <summary>
        /// 获取可等待对象
        /// </summary>
        public HandlerAwaiter<AssetHandler<T>> GetAwaiter()
        {
            if (!IsValid)
            {
                Debug.LogError($"await了一个无效的{GetType().Name}：{Name}");
                return default;
            }
            
            return new HandlerAwaiter<AssetHandler<T>>(this);
        }
        
        public static AssetHandler<T> Create(string name = null,AssetCategory category = AssetCategory.None)
        {
            AssetHandler<T> handler = ReferencePool.Get<AssetHandler<T>>();
            handler.CreateBase(name);
            handler.Category = category;
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
