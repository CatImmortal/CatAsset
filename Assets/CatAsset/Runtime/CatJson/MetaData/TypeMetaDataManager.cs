using System;
using System.Collections.Generic;
using System.Reflection;

namespace CatJson
{
    /// <summary>
    /// 类型元数据管理器
    /// </summary>
    public static class TypeMetaDataManager
    {
        /// <summary>
        /// 用于获取字段/属性的BindingFlags
        /// </summary>
        internal static BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;
        
        /// <summary>
        /// 类型元数据字典
        /// </summary>
        private static Dictionary<Type, TypeMetaData> metaDataDict = new Dictionary<Type, TypeMetaData>();

        /// <summary>
        /// 获取指定类型的元数据，若不存在则创建
        /// </summary>
        private static TypeMetaData GetOrAddMetaData(Type type)
        {
            if (!metaDataDict.TryGetValue(type,out TypeMetaData metaData))
            {
                metaData = new TypeMetaData(type);
                metaDataDict.Add(type,metaData);
            }

            return metaData;
        }
        
        /// <summary>
        /// 添加指定类型需要忽略的成员
        /// </summary>
        public static void AddIgnoreMember(Type type, string memberName)
        {
            TypeMetaData metaData = GetOrAddMetaData(type);
            metaData.AddIgnoreMember(memberName);
        }

        /// <summary>
        /// 是否序列化此类型下的默认值字段/属性
        /// </summary>
        public static bool IsCareDefaultValue(Type type)
        {
            TypeMetaData metaData = GetOrAddMetaData(type);
            return metaData.IsCareDefaultValue;
        }
        
        /// <summary>
        /// 获取指定类型的字段信息
        /// </summary>
        public static Dictionary<RangeString, FieldInfo> GetFieldInfos(Type type)
        {
            TypeMetaData metaData = GetOrAddMetaData(type);
            return metaData.FieldInfos;
        }

        /// <summary>
        /// 获取指定类型的属性信息
        /// </summary>
        public static Dictionary<RangeString, PropertyInfo> GetPropertyInfos(Type type)
        {
            TypeMetaData metaData = GetOrAddMetaData(type);
            return metaData.PropertyInfos;
        }
    }
}