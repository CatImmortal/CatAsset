using System;
using System.Threading;
using UnityEngine;

#if !UNITASK
using System.Threading.Tasks;
#else
using Cysharp.Threading.Tasks;
#endif

namespace CatAsset.Runtime
{
    /// <summary>
    /// 可等待扩展
    /// </summary>
    public static partial class GameObjectPoolManager
    {

        /// <summary>
        /// 使用资源名异步创建对象池，此方法创建的对象池会自动销毁
        /// </summary>
        public static async
#if !UNITASK
              Task<bool>
#else
            UniTask<bool>
#endif

            CreatePoolAsync(string assetName, CancellationToken token = default, CanceledCallback onCanceled = null)
        {
            if (loadedPrefabDict.ContainsKey(assetName))
            {
                //对象池已存在
                return true;
            }

            var handler = CatAssetManager.LoadAssetAsync<GameObject>(assetName, token);
            handler.OnCanceled += onCanceled;
            await handler;

            if (!handler.IsSuccess)
            {
                Debug.LogError($"对象池异步创建失败：{assetName}");
                handler.Unload();
                return false;
            }

            GameObject prefab = handler.Asset;
            loadedPrefabDict[assetName] = prefab;

            //创建对象池
            GameObjectPool pool = GetOrCreatePool(prefab);

            //进行资源绑定
            GameObject root = pool.Root.gameObject;
            root.BindTo(handler);
            return true;

        }

        /// <summary>
        /// 使用资源名异步获取游戏对象
        /// </summary>
        public static async
#if !UNITASK
            Task<GameObject>
#else
            UniTask<GameObject>
#endif

            GetAsync(string assetName, Transform parent = null,
                CancellationToken token = default, CanceledCallback onCanceled = null)
        {
            GameObject go = null;
            if (loadedPrefabDict.TryGetValue(assetName, out var prefab))
            {
                //此对象池已存在
                go = await GetAsync(prefab, parent, token);
                return go;
            }

            bool success = await CreatePoolAsync(assetName, token, onCanceled);
            if (!success)
            {
                return null;
            }

            prefab = loadedPrefabDict[assetName];

            go = await GetAsync(prefab, parent, token, onCanceled);
            return go;
        }

        /// <summary>
        /// 使用模板异步获取游戏对象
        /// </summary>
        public static async
#if !UNITASK
            Task<GameObject>
#else
            UniTask<GameObject>
#endif

            GetAsync(GameObject template, Transform parent = null,
                CancellationToken token = default, CanceledCallback onCanceled = null)
        {
            var pool = GetOrCreatePool(template);
            var handler = pool.GetAsync(parent, token);
            handler.OnCanceled += onCanceled;
            await handler;
            return handler.Instance;
        }

        /// <summary>
        /// 使用资源名异步预热对象
        /// </summary>
        public static async
#if !UNITASK
            Task
#else
            UniTask
#endif
            PrewarmAsync(string assetName, int count, CancellationToken token = default, CanceledCallback onCanceled = null)
        {
            bool hasPool = loadedPrefabDict.TryGetValue(assetName, out var prefab);

            void Prewarm(GameObject template)
            {
                var pool = GetOrCreatePool(template);
                for (int i = 0; i < count; i++)
                {
                    GameObject go = pool.Get(Root);
                    tempList.Add(go);
                }

                //预热完毕 统一释放
                foreach (GameObject go in tempList)
                {
                    Release(template, go);
                }

                tempList.Clear();
            }

            if (!hasPool)
            {
                bool success = await CreatePoolAsync(assetName, token, onCanceled);
                if (success)
                {
                    Prewarm(loadedPrefabDict[assetName]);
                }
            }
            else
            {
                Prewarm(prefab);
            }
        }
    }
}
