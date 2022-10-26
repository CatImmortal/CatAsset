using System;
using System.Collections.Generic;
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
        public static float DefaultObjectExpireTime = 30;

        /// <summary>
        /// 默认对象池失效时间
        /// </summary>
        public static float DefaultPoolExpireTime = 60;

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
        private static Queue<ValueTuple<GameObject, Transform, Action<GameObject>>> waitInstantiateQueue = new Queue<(GameObject, Transform, Action<GameObject>)>();

        /// <summary>
        /// 等待卸载的预制体名字列表
        /// </summary>
        private static List<string> waitUnloadPrefabNames = new List<string>();

        /// <summary>
        /// 轮询游戏对象池管理器
        /// </summary>
        public static void Update(float deltaTime)
        {
            //轮询池子
            foreach (KeyValuePair<GameObject, GameObjectPool> pair in poolDict)
            {
                pair.Value.OnUpdate(deltaTime);
            }

            //销毁长时间未使用的，且是由对象池管理器加载了预制体资源的对象池
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
                var (prefab, parent, callback) = waitInstantiateQueue.Dequeue();
                callback?.Invoke(Object.Instantiate(prefab, parent));
                instantiateCounter++;
            }

            instantiateCounter = 0;
        }

        /// <summary>
        /// 使用预制体名从池中获取一个游戏对象
        /// </summary>
        public static void GetGameObjectAsync(string prefabName, Transform parent, Action<GameObject> callback)
        {
            if (loadedPrefabDict.ContainsKey(prefabName))
            {
                GetGameObjectAsync(loadedPrefabDict[prefabName], parent, callback);
                return;
            }
            
            //此prefab未加载过，先加载
            CatAssetManager.LoadAssetAsync<GameObject>(prefabName, (prefab, result) =>
            {
                if (prefab == null)
                {
                    return;
                }
                
                loadedPrefabDict[prefabName] = prefab;
                
                //这里要先调用GetGameObject 才能保证 poolDict[prefab] 不为空
                GetGameObjectAsync(prefab, parent, callback);
                
                //进行资源绑定
                GameObject root = poolDict[prefab].Root.gameObject;
                CatAssetManager.BindToGameObject(root,prefab);
            });
        }

        /// <summary>
        /// 使用模板中从池中获取一个游戏对象
        /// </summary>
        public static void GetGameObjectAsync(GameObject template, Transform parent, Action<GameObject> callback)
        {
            if (!poolDict.TryGetValue(template, out GameObjectPool pool))
            {
                GameObject root = new GameObject($"Pool-{template.name}");
                root.transform.SetParent(Root);

                pool = new GameObjectPool(template, DefaultObjectExpireTime, root.transform);
                poolDict.Add(template, pool);
            }

            pool.GetGameObjectAsync(parent, callback);
        }

        /// <summary>
        ///  使用预制体名将游戏对象归还池中
        /// </summary>
        public static void ReleaseGameObject(string prefabName, GameObject go)
        {
            if (!loadedPrefabDict.ContainsKey(prefabName))
            {
                return;
            }

            ReleaseGameObject(loadedPrefabDict[prefabName], go);
        }

        /// <summary>
        /// 使用模板将游戏对象归还池中
        /// </summary>
        public static void ReleaseGameObject(GameObject template, GameObject go)
        {
            if (!poolDict.TryGetValue(template, out GameObjectPool pool))
            {
                return;
            }

            pool.ReleaseGameObject(go);
        }

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public static void DestroyPool(string prefabName)
        {
            if (!loadedPrefabDict.ContainsKey(prefabName))
            {
                return;
            }

            DestroyPool(loadedPrefabDict[prefabName]);

            loadedPrefabDict.Remove(prefabName);
        }

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public static void DestroyPool(GameObject template)
        {
            if (!poolDict.TryGetValue(template, out GameObjectPool pool))
            {
                return;
            }

            pool.OnDestroy();
            poolDict.Remove(template);
            Debug.Log($"{template.name}的对象池被销毁了");
        }


        /// <summary>
        /// 分帧异步实例化
        /// </summary>
        public static void InstantiateAsync(GameObject prefab, Transform parent, Action<GameObject> callback)
        {
            waitInstantiateQueue.Enqueue((prefab, parent, callback));
        }

        
        /// <summary>
        /// 预热对象
        /// </summary>
        public static void Prewarm(string prefabName,int count,Action callback)
        {
            List<GameObject> objects = new List<GameObject>(count);
            
            Prewarm(prefabName,count,0,objects, () =>
            {
                foreach (GameObject go in objects)
                {
                    ReleaseGameObject(prefabName,go);
                }
                
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 递归预热对象
        /// </summary>
        private static void Prewarm(string prefabName,int count,int counter, List<GameObject> objects,Action callback)
        {
            GetGameObjectAsync(prefabName,Root,(go =>
            {
                counter++;
                objects.Add(go);
                
                if (counter < count)
                {
                    //预热未结束
                    //递归预热下去
                    Prewarm(prefabName,count,counter,objects,callback);
                }
                else
                {
                    //预热结束
                    callback();
                }
            }));
        }
        
        
        /// <summary>
        /// 预热对象
        /// </summary>
        public static void Prewarm(GameObject template,int count,Action callback)
        {
            Prewarm(template,count,0, () =>
            {
                callback?.Invoke();
            });
        }
        
        /// <summary>
        /// 递归预热对象
        /// </summary>
        private static void Prewarm(GameObject template,int count,int counter,Action callback)
        {
            GetGameObjectAsync(template,Root,(go =>
            {
                counter++;
                ReleaseGameObject(template,go);
                if (counter < count)
                {
                    //预热未结束
                    //递归预热下去
                    Prewarm(template,count,counter,callback);
                }
                else
                {
                    //预热结束
                    callback();
                }
            }));
        }

    }
}
