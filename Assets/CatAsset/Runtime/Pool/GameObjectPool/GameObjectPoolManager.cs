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
        
        //
        // /// <summary>
        // /// 模板->对象池
        // /// </summary>
        // private static Dictionary<GameObject, GameObjectPool> poolDict = new Dictionary<GameObject, GameObjectPool>();

        /// <summary>
        /// 资源名 -> 对象池
        /// </summary>
        private static Dictionary<string, GameObjectPool> poolDict = new Dictionary<string, GameObjectPool>();

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
            foreach (KeyValuePair<string, GameObjectPool> pair in poolDict)
            {
                pair.Value.OnUpdate(deltaTime);
            }

            //销毁长时间未使用的，且是由管理器加载了预制体资源的对象池
            foreach (KeyValuePair<string, GameObject> pair in loadedPrefabDict)
            {
                GameObjectPool pool = poolDict[pair.Key];
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
        private static GameObjectPool GetOrCreatePool(string assetName, GameObject template)
        {
            if (!poolDict.TryGetValue(assetName,out var pool))
            {
                GameObject root = new GameObject($"Pool-{template.name}");
                root.transform.SetParent(Root);

                pool = new GameObjectPool(template, DefaultObjectExpireTime, root.transform);
                poolDict.Add(assetName,pool);
            }

            return pool;
        }
        
        /// <summary>
        /// 异步创建对象池，此方法创建的对象池会自动销毁
        /// </summary>
        public static void CreatePoolAsync(string assetName,Action<bool> callback,CancellationToken token = default)
        {
            if (poolDict.ContainsKey(assetName))
            {
                callback?.Invoke(true);
                return;
            }
            
            CatAssetManager.LoadAssetAsync<GameObject>(assetName,token).OnLoaded += handler =>
            {
                if (!handler.IsSuccess)
                {
                    handler.Unload();
                    callback?.Invoke(false);
                    return;
                }

                GameObject prefab = handler.Asset;
                loadedPrefabDict[assetName] = prefab;
                    
                //创建对象池
                GameObjectPool pool = GetOrCreatePool( assetName,prefab);

                //进行资源绑定
                GameObject root = pool.Root.gameObject;
                root.BindTo(handler);
                    
                callback?.Invoke(true);
            };
            

        }
        
        /// <summary>
        /// 同步创建对象池，此方法创建的对象池需要由创建者销毁
        /// </summary>
        public static void CreatePool(string assetName,GameObject template)
        {
            if (!poolDict.TryGetValue(assetName,out _))
            {
                GetOrCreatePool(assetName,template);
            }
        }

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public static void DestroyPool(string assetName)
        {
            if (!poolDict.TryGetValue(assetName,out var pool))
            {
                return;
            }
            
            pool.OnDestroy();

            loadedPrefabDict.Remove(assetName);
        }
        
        /// <summary>
        /// 异步获取游戏对象，此方法会自动创建未创建的对象池
        /// </summary>
        public static void GetAsync(string assetName, Action<GameObject> callback,Transform parent = null,CancellationToken token = default)
        {
            if (poolDict.TryGetValue(assetName,out var pool))
            {
                //此对象池已存在
                pool.GetAsync(callback,parent,token);
                return;
            }
            
            //不存在 异步创建
            CreatePoolAsync(assetName,(success =>
            {
                if (!success)
                {
                    Debug.LogError($"对象池异步创建失败：{assetName}");
                    callback?.Invoke(null);
                    return;
                }
                
                poolDict[assetName].GetAsync(callback,parent,token);
            }),token);
            
        }
        
        /// <summary>
        /// 同步获取游戏对象，此方法需要先保证对象池已创建
        /// </summary>
        public static GameObject Get(string assetName,Transform parent = null)
        {
            if (!poolDict.TryGetValue(assetName,out var pool))
            {
                Debug.LogError($"同步获取游戏对象的对象池不存在，需要先创建:{assetName}");
                return null;
            }
            
            GameObject go = pool.Get(parent);
            return go;
        }
        
        /// <summary>
        /// 释放游戏对象
        /// </summary>
        public static void Release(string assetName, GameObject go)
        {
            if (!poolDict.TryGetValue(assetName,out var pool))
            {
                Debug.LogWarning($"要释放游戏对象的对象池不存在：{assetName}");
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
        /// 异步预热对象，此方法会自动创建未创建的对象池
        /// </summary>
        public static void PrewarmAsync(string assetName,int count,Action callback,CancellationToken token = default)
        {
            bool hasPool = poolDict.TryGetValue(assetName, out var pool);

            void Prewarm()
            {
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
                        Prewarm();
                    }
                   
                }),token);
            }
            else
            {
                Prewarm();
            }
        }
        
        /// <summary>
        /// 同步预热对象，此方法需要先保证对象池已创建
        /// </summary>
        public static void Prewarm(string assetName,int count)
        {
            if (!poolDict.TryGetValue(assetName,out var pool))
            {
                Debug.LogError($"同步预热游戏对象的对象池不存在，需要先创建:{assetName}");
                return;
            }
            
            for (int i = 0; i < count; i++)
            {
                GameObject go  = pool.Get(Root);
                Release(assetName,go);
            }
        }
        

    }
}
