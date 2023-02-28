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
    public partial class GameObjectPool
    {
        
        /// <summary>
        /// 根节点
        /// </summary>
        public Transform Root { get; }

        /// <summary>
        /// 模板
        /// </summary>
        private GameObject template;
        
        /// <summary>
        /// 对象池失效时间
        /// </summary>
        public float PoolExpireTime;

        /// <summary>
        /// 对象失效时间
        /// </summary>
        public float ObjExpireTime;
        
        /// <summary>
        /// 未使用时间的计时器
        /// </summary>
        public float UnusedTimer { get; private set; }
        
        /// <summary>
        /// 游戏对象 -> 池对象
        /// </summary>
        internal Dictionary<GameObject, PoolObject> PoolObjectDict = new Dictionary<GameObject, PoolObject>();

        /// <summary>
        /// 未被使用的池对象列表
        /// </summary>
        private List<PoolObject> unusedPoolObjectList = new List<PoolObject>();

        /// <summary>
        /// 等待删除的池对象列表
        /// </summary>
        private List<PoolObject> waitRemoveObjectList = new List<PoolObject>();

        public GameObjectPool(GameObject template , Transform root,float poolExpireTime,float objExpireTime)
        {
            this.template = template;
            Root = root;
            PoolExpireTime = poolExpireTime;
            ObjExpireTime = objExpireTime;
        }


        /// <summary>
        /// 轮询对象池
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            foreach (KeyValuePair<GameObject,PoolObject> pair in PoolObjectDict)
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
                    PoolObjectDict.Remove(obj.Target);
                    unusedPoolObjectList.Remove(obj);
                }
                waitRemoveObjectList.Clear();
            }

            //遍历未被使用的池对象
            for (int i = unusedPoolObjectList.Count - 1; i >= 0; i--)
            {
                PoolObject poolObject = unusedPoolObjectList[i];
                poolObject.UnusedTimer += deltaTime;
                if (poolObject.UnusedTimer >= ObjExpireTime && !poolObject.IsLock)
                {
                    //已失效且未锁定 销毁掉
                    PoolObjectDict.Remove(poolObject.Target);
                    unusedPoolObjectList.RemoveAt(i);

                    poolObject.Destroy();
                }
            }

            //判断当前对象池是否正在使用中
            if (PoolObjectDict.Count == 0)
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
            PoolObjectDict.Clear();
        }

        /// <summary>
        /// 锁定游戏对象，被锁定后不会被销毁
        /// </summary>
        public void LockGameObject(GameObject go, bool isLock = true)
        {
            if (!PoolObjectDict.TryGetValue(go, out PoolObject poolObject))
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
                PoolObjectDict.Add(go, poolObject);
                go.SetActive(true);
                return go;
            }

            poolObject = ActivePoolObject(parent);
            return poolObject.Target;
        }

        /// <summary>
        /// 异步获取游戏对象
        /// </summary>
        public InstantiateHandler GetAsync(Transform parent,CancellationToken token)
        {
            InstantiateHandler handler = InstantiateHandler.Create(string.Empty,token, template,parent);

            if (unusedPoolObjectList.Count == 0)
            {
                //没有空闲的池对象
                //实例化游戏对象
                GameObjectPoolManager.InstantiateAsync(handler,PoolObjectDict, (go, userdata) =>
                {
                    var localPoolObjectDict = (Dictionary<GameObject, PoolObject>)userdata;
                    var poolObject = new PoolObject { Target = go,Used = true};
                    localPoolObjectDict.Add(go, poolObject);
                });
            }
            else
            {
                //有空闲的池对象
                //从中拿一个出来
                PoolObject poolObject = ActivePoolObject(parent);
                handler.SetInstance(poolObject.Target);
            }
            return handler;
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

            if (!PoolObjectDict.TryGetValue(go, out PoolObject poolObject))
            {
                Debug.LogWarning($"要释放的对象{go.name}不属于对象池{template.name}");
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
