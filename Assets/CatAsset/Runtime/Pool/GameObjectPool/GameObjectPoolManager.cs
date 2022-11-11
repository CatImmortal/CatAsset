using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 游戏对象池管理器
    /// </summary>
    public static partial class GameObjectPoolManager
    {
        /// <summary>
        /// 异步分帧实例化参数
        /// </summary>
        private struct InstantiateAsyncParam
        {
            public InstantiateAsyncParam(GameObject prefab, Transform parent, CancellationToken token, object userdata1,
                object userdata2, Action<GameObject, object, object> callback)
            {
                Prefab = prefab;
                Parent = parent;
                Token = token;
                Userdata1 = userdata1;
                Userdata2 = userdata2;
                Callback = callback;
            }

            public GameObject Prefab;
            public Transform Parent;
            public CancellationToken Token;
            public object Userdata1;
            public object Userdata2;
            public Action<GameObject,object,object> Callback;
        }


        /// <summary>
        /// 预制体名字->加载好的预制体
        /// </summary>
        private static Dictionary<string, GameObject> loadedPrefabDict = new Dictionary<string, GameObject>();

        /// <summary>
        /// 模板->对象池
        /// </summary>
        private static Dictionary<GameObject, GameObjectPool> poolDict = new Dictionary<GameObject, GameObjectPool>();


        /// <summary>
        /// 游戏对象池管理器的根节点
        /// </summary>
        public static Transform Root;

        /// <summary>
        /// 默认对象失效时间
        /// </summary>
        public static float DefaultObjectExpireTime = 60;

        /// <summary>
        /// 默认对象池失效时间
        /// </summary>
        public static float DefaultPoolExpireTime = 120;

        /// <summary>
        /// 单帧最大实例化数
        /// </summary>
        public static int MaxInstantiateCount = 10;

        /// <summary>
        /// 单帧实例化计数器
        /// </summary>
        private static int instantiateCounter;

        /// <summary>
        /// 等待实例化的游戏对象队列
        /// </summary>
        private static Queue<InstantiateAsyncParam> waitInstantiateQueue = new Queue<InstantiateAsyncParam>();

        /// <summary>
        /// 等待卸载的预制体名字列表
        /// </summary>
        private static List<string> waitUnloadPrefabNames = new List<string>();

        /// <summary>
        /// 轮询管理器
        /// </summary>
        public static void Update(float deltaTime)
        {
            //轮询池子
            foreach (var pair in poolDict)
            {
                pair.Value.OnUpdate(deltaTime);
            }

            //销毁长时间未使用的，且是由管理器加载了预制体资源的对象池
            foreach (KeyValuePair<string, GameObject> pair in loadedPrefabDict)
            {
                GameObjectPool pool = poolDict[pair.Value];
                if (pool.UnusedTimer > DefaultPoolExpireTime)
                {
                    waitUnloadPrefabNames.Add(pair.Key);
                }
            }

            foreach (string prefabName in waitUnloadPrefabNames)
            {
                DestroyPool(prefabName);
            }

            waitUnloadPrefabNames.Clear();

            //处理分帧实例化
            while (instantiateCounter < MaxInstantiateCount && waitInstantiateQueue.Count > 0)
            {
                InstantiateAsyncParam param = waitInstantiateQueue.Dequeue();
                if (param.Token != default && param.Token.IsCancellationRequested)
                {
                    //被取消了
                    continue;
                }
                param.Callback?.Invoke(Object.Instantiate(param.Prefab, param.Parent),param.Userdata1,param.Userdata2);
                instantiateCounter++;
            }

            instantiateCounter = 0;
        }

        /// <summary>
        /// 获取对象池，若不存在则创建
        /// </summary>
        private static GameObjectPool GetOrCreatePool(GameObject template)
        {
            if (!poolDict.TryGetValue(template,out var pool))
            {
                GameObject root = new GameObject($"Pool-{template.name}");
                root.transform.SetParent(Root);

                pool = new GameObjectPool(template, DefaultObjectExpireTime, root.transform);
                poolDict.Add(template,pool);
            }

            return pool;
        }

        /// <summary>
        /// 使用资源名异步创建对象池，此方法创建的对象池会自动销毁
        /// </summary>
        public static void CreatePoolAsync(string assetName,Action<bool> callback,CancellationToken token = default)
        {
            if (loadedPrefabDict.ContainsKey(assetName))
            {
                //对象池已存在
                callback?.Invoke(true);
                return;
            }

            CatAssetManager.LoadAssetAsync<GameObject>(assetName,token).OnLoaded += handler =>
            {
                if (!handler.IsSuccess)
                {
                    Debug.LogError($"对象池异步创建失败：{assetName}");
                    handler.Unload();
                    callback?.Invoke(false);
                    return;
                }

                GameObject prefab = handler.Asset;
                loadedPrefabDict[assetName] = prefab;

                //创建对象池
                GameObjectPool pool = GetOrCreatePool(prefab);

                //进行资源绑定
                GameObject root = pool.Root.gameObject;
                root.BindTo(handler);

                callback?.Invoke(true);
            };


        }

        /// <summary>
        /// 使用模板同步创建对象池，此方法创建的对象池需要由创建者销毁
        /// </summary>
        public static void CreatePool(GameObject template)
        {
            GetOrCreatePool(template);
        }

        /// <summary>
        /// 使用资源名销毁对象池
        /// </summary>
        public static void DestroyPool(string assetName)
        {
            if (!loadedPrefabDict.TryGetValue(assetName,out var prefab))
            {
                return;
            }

            var pool = GetOrCreatePool(prefab);
            pool.OnDestroy();

            poolDict.Remove(prefab);
            loadedPrefabDict.Remove(assetName);
        }

        /// <summary>
        /// 使用模板销毁对象池
        /// </summary>
        public static void DestroyPool(GameObject template)
        {
            poolDict.Remove(template);
        }

        /// <summary>
        /// 使用资源名异步获取游戏对象
        /// </summary>
        public static void GetAsync(string assetName, Action<GameObject> callback,Transform parent = null,CancellationToken token = default)
        {
            if (loadedPrefabDict.TryGetValue(assetName,out var prefab))
            {
                //此对象池已存在
                GetAsync(prefab, callback, parent, token);
                return;
            }

            //此对象池不存在 创建对象池
            CreatePoolAsync(assetName,(success =>
            {
                if (!success)
                {
                    callback?.Invoke(null);
                    return;
                }

                prefab = loadedPrefabDict[assetName];

                GetAsync(prefab, callback, parent, token);

            }),token);
        }

        /// <summary>
        /// 使用模板异步获取游戏对象
        /// </summary>
        public static void GetAsync(GameObject template, Action<GameObject> callback, Transform parent = null,
            CancellationToken token = default)
        {
            var pool = GetOrCreatePool(template);
            pool.GetAsync(callback,parent,token);
        }

        /// <summary>
        /// 使用资源名同步获取游戏对象，此方法需要先保证对象池已创建
        /// </summary>
        public static GameObject Get(string assetName,Transform parent = null)
        {
            if (!loadedPrefabDict.TryGetValue(assetName,out var prefab))
            {
                Debug.LogError($"使用资源名同步获取游戏对象的对象池不存在，需要先创建:{assetName}");
                return null;
            }

            return Get(prefab, parent);
        }

        /// <summary>
        /// 使用模板同步获取游戏对象
        /// </summary>
        public static GameObject Get(GameObject template, Transform parent = null)
        {
            var pool = GetOrCreatePool(template);
            GameObject go = pool.Get(parent);
            return go;
        }

        /// <summary>
        /// 使用资源名释放游戏对象
        /// </summary>
        public static void Release(string assetName, GameObject go)
        {
            if (!loadedPrefabDict.TryGetValue(assetName,out var prefab))
            {
                Debug.LogWarning($"要释放游戏对象的对象池不存在：{assetName}");
                return;
            }

            Release(prefab,go);
        }

        /// <summary>
        /// 使用模板释放游戏对象
        /// </summary>
        public static void Release(GameObject template, GameObject go)
        {
            if (!poolDict.TryGetValue(template, out var pool))
            {
                Debug.LogWarning($"要释放游戏对象的对象池不存在：{go.name}");
                return;
            }
            pool.Release(go);
        }


        /// <summary>
        /// 分帧异步实例化
        /// </summary>
        internal static void InstantiateAsync(GameObject prefab, Transform parent,CancellationToken token,object userdata1,object userdata2, Action<GameObject,object,object> callback)
        {
            waitInstantiateQueue.Enqueue(new InstantiateAsyncParam(prefab, parent, token, userdata1,userdata2, callback));
        }

        /// <summary>
        /// 使用资源名异步预热对象
        /// </summary>
        public static void PrewarmAsync(string assetName,int count,Action callback,CancellationToken token = default)
        {
            bool hasPool = loadedPrefabDict.TryGetValue(assetName,out var prefab);

            void Prewarm(GameObject template)
            {
                var pool = GetOrCreatePool(template);
                for (int i = 0; i < count; i++)
                {
                    GameObject go  = pool.Get(Root);
                    Release(assetName,go);
                }
                callback?.Invoke();
            }

            if (!hasPool)
            {
                CreatePoolAsync(assetName,(result =>
                {
                    if (result)
                    {
                        Prewarm(loadedPrefabDict[assetName]);
                    }

                }),token);
            }
            else
            {
                Prewarm(prefab);
            }
        }
    }
}
