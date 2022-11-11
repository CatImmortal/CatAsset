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
#if !UNITASK
        /// <summary>
        /// 使用资源名异步创建对象池，此方法创建的对象池会自动销毁
        /// </summary>
        public static Task<bool> CreatePoolAsync(string assetName, CancellationToken token = default)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CreatePoolAsync(assetName, (result =>
            {
                tcs.TrySetResult(result);
            }),token);
            return tcs.Task;
        }

        /// <summary>
        /// 使用资源名异步获取游戏对象
        /// </summary>
        public static Task<GameObject> GetAsync(string assetName, Transform parent = null,
            CancellationToken token = default)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();

            GetAsync(assetName, (go =>
            {
                tcs.TrySetResult(go);
            }), parent, token);
            
            return tcs.Task;
        }

        /// <summary>
        /// 使用模板异步获取游戏对象
        /// </summary>
        public static Task<GameObject> GetAsync(GameObject template, Transform parent = null,
            CancellationToken token = default)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();

            GetAsync(template, (go =>
            {
                tcs.TrySetResult(go);
            }), parent, token);
            
            return tcs.Task;
        }
        
        /// <summary>
        /// 使用资源名异步预热对象
        /// </summary>
        public static Task PrewarmAsync(string assetName, int count, CancellationToken token = default)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            PrewarmAsync(assetName, count, () =>
            {
                tcs.TrySetResult(null);
            }, token);
            
            return tcs.Task;
        }
#else 
        /// <summary>
        /// 使用资源名异步创建对象池，此方法创建的对象池会自动销毁
        /// </summary>
        public static UniTask<bool> CreatePoolAsync(string assetName, CancellationToken token = default)
        {
            UniTaskCompletionSource<bool> tcs = new UniTaskCompletionSource<bool>();
            CreatePoolAsync(assetName, (result =>
            {
                tcs.TrySetResult(result);
            }),token);
            return tcs.Task;
        }

        /// <summary>
        /// 使用资源名异步获取游戏对象
        /// </summary>
        public static UniTask<GameObject> GetAsync(string assetName, Transform parent = null,
            CancellationToken token = default)
        {
            UniTaskCompletionSource<GameObject> tcs = new UniTaskCompletionSource<GameObject>();

            GetAsync(assetName, (go =>
            {
                tcs.TrySetResult(go);
            }), parent, token);
            
            return tcs.Task;
        }

        /// <summary>
        /// 使用模板异步获取游戏对象
        /// </summary>
        public static UniTask<GameObject> GetAsync(GameObject template, Transform parent = null,
            CancellationToken token = default)
        {
            UniTaskCompletionSource<GameObject> tcs = new UniTaskCompletionSource<GameObject>();

            GetAsync(template, (go =>
            {
                tcs.TrySetResult(go);
            }), parent, token);
            
            return tcs.Task;
        }
        
        /// <summary>
        /// 使用资源名异步预热对象
        /// </summary>
        public static UniTask PrewarmAsync(string assetName, int count, CancellationToken token = default)
        {
            UniTaskCompletionSource<object> tcs = new UniTaskCompletionSource<object>();

            PrewarmAsync(assetName, count, () =>
            {
                tcs.TrySetResult(null);
            }, token);
            
            return tcs.Task;
        }
#endif
        
        
    }
}
