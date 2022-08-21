using System;
using System.Collections.Generic;
using System.Reflection;

namespace CatJson
{
    /// <summary>
    /// 类型的反射元数据
    /// </summary>
    public class TypeMetaData
    {

        /// <summary>
        /// 类型信息
        /// </summary>
        private Type type;

        /// <summary>
        /// 是否序列化此类型下的默认值字段/属性
        /// </summary>
        public bool IsCareDefaultValue { get; }
        
        /// <summary>
        /// 字段信息
        /// </summary>
        public Dictionary<RangeString, FieldInfo> FieldInfos { get; } = new Dictionary<RangeString, FieldInfo>();

        /// <summary>
        /// 属性信息
        /// </summary>
        public Dictionary<RangeString, PropertyInfo> PropertyInfos { get; } = new Dictionary<RangeString, PropertyInfo>();

        /// <summary>
        /// 需要忽略处理的字段/属性名
        /// </summary>
        private HashSet<string> ignoreMembers = new HashSet<string>();
        
        public TypeMetaData(Type type)
        {
            this.type = type;
            IsCareDefaultValue = Attribute.IsDefined(type, typeof(JsonCareDefaultValueAttribute));
            
            //收集字段信息
            FieldInfo[] fis = type.GetFields(TypeMetaDataManager.Flags);
            foreach (FieldInfo fi in fis)
            {
                if (IsIgnoreMember(fi,fi.Name))
                {
                    continue;
                }
                
                FieldInfos.Add(new RangeString(fi.Name), fi);
            }
            
            //收集属性信息
            PropertyInfo[] pis = type.GetProperties(TypeMetaDataManager.Flags);
            foreach (PropertyInfo pi in pis)
            {
                if (IsIgnoreMember(pi,pi.Name))
                {
                    continue;
                }
                
                if (pi.SetMethod != null && pi.GetMethod != null && pi.Name != "Item")
                {
                    //属性必须同时具有get set 并且不能是索引器item
                    PropertyInfos.Add(new RangeString(pi.Name),pi);
                }
            }
            
          
        }

        /// <summary>
        /// 是否需要忽略此字段/属性
        /// </summary>
        private bool IsIgnoreMember(MemberInfo mi,string name)
        {
            if (Attribute.IsDefined(mi, typeof(JsonIgnoreAttribute)))
            {
                return true;
            }

            if (ignoreMembers.Contains(name))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 添加需要忽略的成员
        /// </summary>
        public void AddIgnoreMember(string memberName)
        {
            ignoreMembers.Add(memberName);
        }
        
        public override string ToString()
        {
            return type.ToString();
        }
    }
}