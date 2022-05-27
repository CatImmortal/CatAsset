using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 引用池
    /// </summary>
    public static partial class ReferencePool
    {
        private static Dictionary<Type, Pool> poolDict = new Dictionary<Type, Pool>();

        /// <summary>
        /// 从池中获取引用
        /// </summary>
        public static T Get<T>() where T : IReference,new ()
        {
            Pool pool = GetOrCreatePool(typeof(T));
            return pool.Get<T>();
        }

        /// <summary>
        /// 将引用归还池中
        /// </summary>
        public static void Release(IReference reference)
        {
            Pool pool = GetOrCreatePool(reference.GetType());
            pool.Release(reference);
        }

        /// <summary>
        /// 获取引用池（若不存在则创建）
        /// </summary>
        private static Pool GetOrCreatePool(Type type)
        {
            if (!poolDict.TryGetValue(type,out var pool))
            {
                pool = new Pool();
                poolDict.Add(type,pool);
            }

            return pool;
        }
    }
}