using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    public static partial class ReferencePool
    {
        /// <summary>
        /// 池
        /// </summary>
        private class Pool
        {
            private readonly List<IReference> list = new List<IReference>();

            public T Get<T>() where T : IReference,new ()
            {
                if (list.Count == 0)
                {
                    T obj = new T();
                    return obj;
                }

                IReference reference = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                
                return (T) reference;
            }

            public void Release(IReference reference)
            {
                reference.Clear();
                list.Add(reference);
            }
        }
    }
  
}