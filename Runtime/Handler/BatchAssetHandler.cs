using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 批量资源加载完毕回调方法的原型
    /// </summary>
    public delegate void BatchAssetLoadedCallback(List<AssetHandler<object>> handlers);

    /// <summary>
    /// 批量资源句柄
    /// </summary>
    public class BatchAssetHandler : BaseHandler
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
        /// 资源句柄列表
        /// </summary>
        internal readonly List<AssetHandler<object>> Handlers  = new List<AssetHandler<object>>();

        /// <summary>
        /// 资源加载完毕回调
        /// </summary>
        internal readonly AssetLoadedCallback<object> OnAssetLoadedCallback;

        /// <inheritdoc />
        public override bool Success => loadedCount == assetCount;

        /// <summary>
        /// 批量资源加载完毕回调
        /// </summary>
        private BatchAssetLoadedCallback onLoadedCallback;

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
            if (loadedCount == assetCount)
            {
                IsDone = true;
                onLoadedCallback?.Invoke(Handlers);
                ContinuationCallBack?.Invoke();
                
                //加载结束 释放句柄
                if (IsValid)
                {
                    Release();
                }
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
            if (!IsValid)
            {
                Debug.LogWarning($"取消了无效的{GetType().Name}");
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
            if (!IsValid)
            {
                Debug.LogError($"卸载了无效的{GetType().Name}");
                return;
            }
            
            foreach (AssetHandler<object> assetHandler in Handlers)
            {
                if (!assetHandler.IsValid)
                {
                    continue;
                }

                assetHandler.Unload();
            }

            //释放自身
            Release();
        }

        public static BatchAssetHandler Create(int assetCount,BatchAssetLoadedCallback callback)
        {
            BatchAssetHandler handler = ReferencePool.Get<BatchAssetHandler>();
            handler.IsValid = true;
            handler.assetCount = assetCount;
            handler.onLoadedCallback = callback;
            handler.IsDone = assetCount == 0;

            if (handler.IsDone)
            {
                handler.onLoadedCallback?.Invoke(handler.Handlers);
                handler.ContinuationCallBack?.Invoke();
                
                //加载结束 释放句柄
                handler.Release();
            }
            
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
