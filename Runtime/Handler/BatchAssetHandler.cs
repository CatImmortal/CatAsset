using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = System.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 批量资源加载完毕回调方法的原型
    /// </summary>
    public delegate void BatchAssetLoadedCallback(BatchAssetHandler handler);

    /// <summary>
    /// 批量资源句柄
    /// </summary>
    public class BatchAssetHandler : BaseHandler ,IBindableHandler
    {
        /// <summary>
        /// 需要加载的资源数量
        /// </summary>
        private int assetCount;

        /// <summary>
        /// 加载结束的资源数量
        /// </summary>
        private int loadedCount;

        /// <summary>
        /// 资源句柄列表，注意：会在加载结束调用完回调后被清空
        /// </summary>
        public List<AssetHandler<object>> Handlers { get; } = new List<AssetHandler<object>>();

        /// <summary>
        /// 资源加载完毕回调
        /// </summary>
        internal readonly AssetLoadedCallback<object> OnAssetLoadedCallback;

        /// <summary>
        /// 批量资源加载完毕回调
        /// </summary>
        private BatchAssetLoadedCallback onLoadedCallback;

        /// <summary>
        /// 批量资源加载完毕回调
        /// </summary>
        public event BatchAssetLoadedCallback OnLoaded
        {
            add
            {
                if (State == HandlerState.InValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上添加了OnLoaded回调");
                    return;
                }

                if (State != HandlerState.Doing)
                {
                    value?.Invoke(this);
                    return;
                }

                onLoadedCallback += value;
            }

            remove
            {
                if (State == HandlerState.InValid)
                {
                    Debug.LogError($"在无效的{GetType().Name}：{Name}上移除了OnLoaded回调");
                    return;
                }

                onLoadedCallback -= value;
            }
        }
        
        public BatchAssetHandler()
        {
            OnAssetLoadedCallback = OnAssetLoaded;
        }

        /// <summary>
        /// 资源加载完毕回调
        /// </summary>
        private void OnAssetLoaded(AssetHandler<object> handler)
        {
            loadedCount++;
            
            CheckLoaded();
        }

        /// <summary>
        /// 检查所有资源是否已加载完毕
        /// </summary>
        internal void CheckLoaded()
        {
            if (loadedCount == assetCount)
            {
                State = HandlerState.Success;
            
                onLoadedCallback?.Invoke(this);
                ContinuationCallBack?.Invoke();
            }
        }

        /// <summary>
        /// 添加资源句柄
        /// </summary>
        internal void AddAssetHandler(AssetHandler<object> handler)
        {
            Handlers.Add(handler);
        }
        
        /// <inheritdoc />
        public override void Cancel()
        {
            if (State == HandlerState.InValid)
            {
                Debug.LogWarning($"取消了无效的{GetType().Name}：{Name}");
                return;
            }
            
            foreach (AssetHandler<object> assetHandler in Handlers)
            {
                assetHandler.Dispose();
            }

            //释放自身
            Release();
        }

        /// <inheritdoc />
        public override void Unload()
        {
            if (State == HandlerState.InValid)
            {
                Debug.LogError($"卸载了无效的{GetType().Name}：{Name}");
                return;
            }
            
            foreach (AssetHandler<object> assetHandler in Handlers)
            {
                assetHandler.Dispose();
            }

            //释放自身
            Release();
        }

        /// <summary>
        /// 获取可等待对象
        /// </summary>
        public HandlerAwaiter<BatchAssetHandler> GetAwaiter()
        {
            return new HandlerAwaiter<BatchAssetHandler>(this);
        }
        
        public static BatchAssetHandler Create(int assetCount = 0)
        {
            BatchAssetHandler handler = ReferencePool.Get<BatchAssetHandler>();
            handler.State = HandlerState.Doing;
            handler.assetCount = assetCount;
            
            handler.CheckLoaded();
            
            return handler;
        }

        public override void Clear()
        {
            base.Clear();

            assetCount = default;
            loadedCount = default;
            Handlers.Clear();
        }
    }
}
