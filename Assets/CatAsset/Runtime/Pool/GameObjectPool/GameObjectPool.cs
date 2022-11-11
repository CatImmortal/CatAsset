using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 游戏对象池
    /// </summary>
    public class GameObjectPool
    {
        /// <summary>
        /// 未使用时间的计时器
        /// </summary>
        public float UnusedTimer { get; private set; }

        /// <summary>
        /// 根节点
        /// </summary>
        public Transform Root { get; }

        /// <summary>
        /// 模板
        /// </summary>
        private GameObject template;

        /// <summary>
        /// 对象失效时间
        /// </summary>
        private float expireTime;

        /// <summary>
        /// 游戏对象 -> 池对象
        /// </summary>
        private Dictionary<GameObject, PoolObject> poolObjectDict = new Dictionary<GameObject, PoolObject>();

        /// <summary>
        /// 未被使用的池对象列表
        /// </summary>
        private List<PoolObject> unusedPoolObjectList = new List<PoolObject>();

        /// <summary>
        /// 等待删除的池对象列表
        /// </summary>
        private List<PoolObject> waitRemoveObjectList = new List<PoolObject>();

        public GameObjectPool(GameObject template, float expireTime, Transform root)
        {
            this.template = template;
            this.expireTime = expireTime;
            Root = root;
        }


        /// <summary>
        /// 轮询对象池
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            foreach (KeyValuePair<GameObject,PoolObject> pair in poolObjectDict)
            {
                if (pair.Value.Target == null)
                {
                    //Target被意外销毁了 要移除掉
                    waitRemoveObjectList.Add(pair.Value);
                }
            }
            if (waitRemoveObjectList.Count > 0)
            {
                foreach (PoolObject obj in waitRemoveObjectList)
                {
                    poolObjectDict.Remove(obj.Target);
                    unusedPoolObjectList.Remove(obj);
                }
                waitRemoveObjectList.Clear();
            }

            //遍历未被使用的池对象
            for (int i = unusedPoolObjectList.Count - 1; i >= 0; i--)
            {
                PoolObject poolObject = unusedPoolObjectList[i];
                poolObject.UnusedTimer += deltaTime;
                if (poolObject.UnusedTimer >= expireTime && !poolObject.IsLock)
                {
                    //已失效且未锁定 销毁掉
                    poolObjectDict.Remove(poolObject.Target);
                    unusedPoolObjectList.RemoveAt(i);

                    poolObject.Destroy();
                }
            }

            //判断当前对象池是否正在使用中
            if (poolObjectDict.Count == 0)
            {
                UnusedTimer += deltaTime;
            }
            else
            {
                UnusedTimer = 0;
            }
        }


        /// <summary>
        /// 销毁对象池
        /// </summary>
        public void OnDestroy()
        {
            Clear();
            template = null;
            GameObject.Destroy(Root.gameObject);
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < unusedPoolObjectList.Count; i++)
            {
                unusedPoolObjectList[i].Destroy();
            }

            unusedPoolObjectList.Clear();
            poolObjectDict.Clear();
        }

        /// <summary>
        /// 锁定游戏对象，被锁定后不会被销毁
        /// </summary>
        public void LockGameObject(GameObject go, bool isLock = true)
        {
            if (!poolObjectDict.TryGetValue(go, out PoolObject poolObject))
            {
                return;
            }

            poolObject.IsLock = true;
        }

        /// <summary>
        /// 同步获取游戏对象
        /// </summary>
        public GameObject Get(Transform parent)
        {
            PoolObject poolObject;
            if (unusedPoolObjectList.Count == 0)
            {
                GameObject go = Object.Instantiate(template, parent);
                poolObject = new PoolObject { Target = go, Used = true };
                poolObjectDict.Add(go, poolObject);
                go.SetActive(true);
                return go;
            }
            
            poolObject = ActivePoolObject(parent);
            return poolObject.Target;
        }
        
        /// <summary>
        /// 异步获取游戏对象
        /// </summary>
        public void GetAsync(Action<GameObject> callback,Transform parent,CancellationToken token)
        {
            if (unusedPoolObjectList.Count == 0)
            {
                //没有未使用的池对象，需要实例化出来
                GameObjectPoolManager.InstantiateAsync(template, parent, token, poolObjectDict, callback,
                    (go, userdata1, userdata2) =>
                    {
                        var localPoolObjectDict = (Dictionary<GameObject, PoolObject>)userdata1;
                        var localCallback = (Action<GameObject>)userdata2;

                        PoolObject poolObject = new PoolObject { Target = go, Used = true };

                        localPoolObjectDict.Add(go, poolObject);

                        go.SetActive(true);
                        localCallback?.Invoke(go);
                    });

                return;
            }

            //有空闲的池对象
            //从中拿一个出来
            PoolObject poolObject = ActivePoolObject(parent);
            callback?.Invoke(poolObject.Target);
        }
        
        /// <summary>
        /// 激活一个池对象
        /// </summary>
        private PoolObject ActivePoolObject(Transform parent)
        {
            PoolObject poolObject = unusedPoolObjectList[unusedPoolObjectList.Count - 1];
            unusedPoolObjectList.RemoveAt(unusedPoolObjectList.Count - 1);
            poolObject.Target.transform.SetParent(parent);
            poolObject.Target.SetActive(true);
            return poolObject;
        }
        
        /// <summary>
        /// 释放游戏对象
        /// </summary>
        public void Release(GameObject go)
        {
            if (go == null)
            {
                return;
            }
            
            if (!poolObjectDict.TryGetValue(go, out PoolObject poolObject))
            {
                return;
            }

            poolObject.Used = false;
            poolObject.UnusedTimer = 0;
            go.SetActive(false);
            go.transform.SetParent(Root);

            unusedPoolObjectList.Add(poolObject);
        }
    }
}
