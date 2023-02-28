using System;
using System.Collections.Generic;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 分析器对象池信息
    /// </summary>
    [Serializable]
    public class ProfilerPoolInfo : IReference,IComparable<ProfilerPoolInfo>
    {
        /// <summary>
        /// 池对象信息
        /// </summary>
        public class PoolObjectInfo: IReference
        {
            /// <summary>
            /// 实例ID
            /// </summary>
            public int InstanceID;
            
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

            public static PoolObjectInfo Create(int instanceID, bool used,float unusedTimer,bool isLock)
            {
                PoolObjectInfo info = ReferencePool.Get<PoolObjectInfo>();
                info.InstanceID = instanceID;
                info.Used = used;
                info.UnusedTimer = unusedTimer;
                info.IsLock = isLock;
                return info;
            }
            
            public void Clear()
            {
                InstanceID = default;
                Used = default;
                UnusedTimer = default;
                IsLock = default;
            }

        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name;
        
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
        public float UnusedTimer;
        
        /// <summary>
        /// 总对象数
        /// </summary>
        public int AllCount;

        /// <summary>
        /// 已使用对象数
        /// </summary>
        public int UsedCount;
        
        /// <summary>
        /// 未使用对象数
        /// </summary>
        public int UnusedCount;
        
        /// <summary>
        /// 池对象列表
        /// </summary>
        public List<PoolObjectInfo> PoolObjectList = new List<PoolObjectInfo>();

        public static ProfilerPoolInfo Create(string name, float poolExpireTime, float objExpireTime, float unusedTimer,
            int allCount)
        {
            ProfilerPoolInfo info = ReferencePool.Get<ProfilerPoolInfo>();
            info.Name = name;
            info.PoolExpireTime = poolExpireTime;
            info.ObjExpireTime = objExpireTime;
            info.UnusedTimer = unusedTimer;
            info.AllCount = allCount;
            return info;
        }
        
        public void Clear()
        {
            Name = default;
            UnusedTimer = default;
            PoolExpireTime = default;
            ObjExpireTime = default;
            AllCount = default;
            UsedCount = default;
            UnusedCount = default;
            foreach (var info in PoolObjectList)
            {
                ReferencePool.Release(info);
            }
            PoolObjectList.Clear();
        }

        public int CompareTo(ProfilerPoolInfo other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}