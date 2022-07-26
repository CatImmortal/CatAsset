using System;
using System.Collections.Generic;
using UnityEngine;

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
        public Transform Root { get; private set; }

        /// <summary>
        /// 模板
        /// </summary>
        private GameObject template;

        /// <summary>
        /// 对象失效时间
        /// </summary>
        private float expireTime;

        /// <summary>
        /// 游戏对象->池对象
        /// </summary>
        private Dictionary<GameObject, PoolObject> poolObjectDict = new Dictionary<GameObject, PoolObject>();

        /// <summary>
        /// 未被使用的池对象列表
        /// </summary>
        private List<PoolObject> unusedPoolObjectList = new List<PoolObject>();


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
            for (int i = unusedPoolObjectList.Count - 1; i >= 0; i--)
            {
                PoolObject poolObject = unusedPoolObjectList[i];
                poolObject.UnusedTimer += deltaTime;
                if (poolObject.UnusedTimer >= expireTime && !poolObject.IsLock)
                {
                    //已过期且未锁定 销毁掉
                    poolObjectDict.Remove(poolObject.Target);
                    unusedPoolObjectList.RemoveAt(i);

                    poolObject.Destroy();
                }
            }

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
        /// 从池中获取一个游戏对象
        /// </summary>
        public void GetGameObject(Transform parent, Action<GameObject> callback)
        {
            if (unusedPoolObjectList.Count == 0)
            {
                //没有未使用的池对象，需要实例化出来
                GameObjectPoolManager.InstantiateAsync(template, parent, (go) =>
                {
                    PoolObject poolObject = new PoolObject { Target = go, Used = true };

                    poolObjectDict.Add(go, poolObject);

                    go.SetActive(true);
                    callback?.Invoke(go);
                });

                return;
            }

            //有空闲的池对象
            //从中拿一个出来
            PoolObject poolObject = unusedPoolObjectList[unusedPoolObjectList.Count - 1];
            unusedPoolObjectList.RemoveAt(unusedPoolObjectList.Count - 1);
            poolObject.Target.transform.SetParent(parent);
            poolObject.Target.SetActive(true);
            callback?.Invoke(poolObject.Target);
        }

        /// <summary>
        /// 将游戏对象归还池中
        /// </summary>
        public void ReleaseGameObject(GameObject go)
        {
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
