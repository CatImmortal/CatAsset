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
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static Task<GameObject> GetGameObjectAsync(string prefabName, Transform parent)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            GetGameObjectAsync(prefabName, parent, (go) =>
            {
                tcs.SetResult(go);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static Task<GameObject> GetGameObjectAsync(GameObject template, Transform parent)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            GetGameObjectAsync(template, parent, (go) =>
            {
                tcs.SetResult(go);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 预热对象（可等待）
        /// </summary>
        public static Task Prewarm(string prefabName, int count)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Prewarm(prefabName,count, () =>
            {
                tcs.SetResult(null);   
            });
            return tcs.Task;
        }
        
        /// <summary>
        /// 预热对象（可等待）
        /// </summary>
        public static Task Prewarm(GameObject template, int count)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Prewarm(template,count, () =>
            {
                tcs.SetResult(null);   
            });
            return tcs.Task;
        }
#else
        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static UniTask<GameObject> GetGameObjectAsync(string prefabName, Transform parent)
        {
            UniTaskCompletionSource<GameObject> tcs = new UniTaskCompletionSource<GameObject>();
            GetGameObjectAsync(prefabName, parent, (go) =>
            {
                tcs.TrySetResult(go);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static UniTask<GameObject> GetGameObjectAsync(GameObject template, Transform parent)
        {
            UniTaskCompletionSource<GameObject> tcs = new UniTaskCompletionSource<GameObject>();
            GetGameObjectAsync(template, parent, (go) =>
            {
                tcs.TrySetResult(go);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 预热对象（可等待）
        /// </summary>
        public static UniTask Prewarm(string prefabName, int count)
        {
            UniTaskCompletionSource<object> tcs = new UniTaskCompletionSource<object>();
            Prewarm(prefabName,count, () =>
            {
                tcs.TrySetResult(null);   
            });
            return tcs.Task;
        }
        
        /// <summary>
        /// 预热对象（可等待）
        /// </summary>
        public static UniTask Prewarm(GameObject template, int count)
        {
            UniTaskCompletionSource<object> tcs = new UniTaskCompletionSource<object>();
            Prewarm(template,count, () =>
            {
                tcs.TrySetResult(null);   
            });
            return tcs.Task;
        }
#endif
        
        
    }
}
