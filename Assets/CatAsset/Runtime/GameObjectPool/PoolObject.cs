using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 池对象
    /// </summary>
    public class PoolObject
    {
        /// <summary>
        /// 包装的游戏对象
        /// </summary>
        public GameObject Target;

        /// <summary>
        /// 是否被使用了
        /// </summary>
        public bool Used;

        /// <summary>
        /// 未使用计时
        /// </summary>
        public float UnusedTimer;

        /// <summary>
        /// 是否被锁定，被锁定的池对象不会被销毁
        /// </summary>
        public bool IsLock;

        public void Destroy()
        {
            Object.Destroy(Target);
        }
    }
}
