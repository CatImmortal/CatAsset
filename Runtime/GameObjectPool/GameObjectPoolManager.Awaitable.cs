using System.Threading.Tasks;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 可等待扩展
    /// </summary>
    public static partial class GameObjectPoolManager
    {
        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static Task<GameObject> AwaitGetGameObject(string prefabName, Transform parent)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            GetGameObject(prefabName, parent, (go) =>
            {
                tcs.SetResult(go);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static Task<GameObject> AwaitGetGameObject(GameObject template, Transform parent)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            GetGameObject(template, parent, (go) =>
            {
                tcs.SetResult(go);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 预热对象（可等待）
        /// </summary>
        public static Task AwaitPrewarm(string prefabName, int count)
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
        public static Task AwaitPrewarm(GameObject template, int count)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Prewarm(template,count, () =>
            {
                tcs.SetResult(null);   
            });
            return tcs.Task;
        }
    }
}
