using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 批量资源加载完毕回调方法的原型
    /// </summary>
    public delegate void BatchAssetLoadedCallback(BatchAssetHandler handler);

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
        public readonly List<AssetHandler<object>> Handlers  = new List<AssetHandler<object>>();

        /// <summary>
        /// 资源加载完毕回调
        /// </summary>
        private readonly AssetLoadedCallback<object> onAssetLoaded;

        /// <inheritdoc />
        public override bool Success => loadedCount == assetCount;

        /// <summary>
        /// 批量资源加载完毕回调
        /// </summary>
        private BatchAssetLoadedCallback onLoaded;

        /// <summary>
        /// 批量资源加载完毕回调
        /// </summary>
        public event BatchAssetLoadedCallback OnLoaded
        {
            add
            {
                if (!IsValid)
                {
                    Debug.LogError($"错误的在无效的{GetType().Name}上添加OnLoaded回调");
                    return;
                }

                if (IsDone)
                {
                    value?.Invoke(this);
                    return;
                }

                onLoaded += value;
            }

            remove
            {
                if (!IsValid)
                {
                    Debug.LogError($"错误的在无效的{GetType().Name}上移除OnLoaded回调");
                    return;
                }

                onLoaded -= value;
            }
        }

        public BatchAssetHandler()
        {
            onAssetLoaded = OnAssetLoaded;
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
                onLoaded?.Invoke(this);
                AwaiterContinuation?.Invoke();
            }
        }

        /// <summary>
        /// 添加资源句柄
        /// </summary>
        internal void AddAssetHandler(AssetHandler<object> handler)
        {
            Handlers.Add(handler);
            handler.OnLoaded += onAssetLoaded;
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

        public static BatchAssetHandler Create(int assetCount)
        {
            BatchAssetHandler handler = ReferencePool.Get<BatchAssetHandler>();
            handler.IsValid = true;
            handler.assetCount = assetCount;
            handler.IsDone = assetCount == 0;
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
