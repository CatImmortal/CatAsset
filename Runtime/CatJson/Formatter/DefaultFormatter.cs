using System;
using System.Collections.Generic;
using System.Reflection;

namespace CatJson
{
    /// <summary>
    /// 默认的json格式化器，会通过反射来序列化/反序列化字段/属性
    /// </summary>
    public class DefaultFormatter : IJsonFormatter
    {
        /// <summary>
        /// 类型与其对应的属性信息
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<RangeString, PropertyInfo>> propertyInfoDict = new Dictionary<Type, Dictionary<RangeString, PropertyInfo>>();
        
        /// <summary>
        /// 类型与其对应的字段信息
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<RangeString, FieldInfo>> fieldInfoDict = new Dictionary<Type, Dictionary<RangeString, FieldInfo>>();

        /// <summary>
        /// 需要忽略的类型字段/属性名称
        /// </summary>
        private static Dictionary<Type, HashSet<string>> ignoreSet = new Dictionary<Type, HashSet<string>>();
        
        /// <inheritdoc />
        public void ToJson(object value, Type type, Type realType, int depth)
        {
            if (!propertyInfoDict.ContainsKey(realType) && !fieldInfoDict.ContainsKey(realType))
            {
                //初始化反射信息
                AddReflectionInfo(realType);
            }
            
            //是否需要删除最后一个逗号
            bool needRemoveLastComma = false;
            
            TextUtil.AppendLine("{");
            
            //序列化属性
            foreach (KeyValuePair<RangeString, PropertyInfo> item in propertyInfoDict[realType])
            {
                object propValue = item.Value.GetValue(value);
                
                if (JsonParser.IgnoreDefaultValue && TypeUtil.IsDefaultValue(propValue))
                {
                    //默认值跳过序列化
                    continue;
                }
                
                AppendMember(item.Value.PropertyType,item.Value.Name,propValue,depth + 1);
                needRemoveLastComma = true;
            }
            
            //序列化字段
            foreach (KeyValuePair<RangeString, FieldInfo> item in fieldInfoDict[realType])
            {
                object fieldValue = item.Value.GetValue(value);

                if (JsonParser.IgnoreDefaultValue && TypeUtil.IsDefaultValue(fieldValue))
                {
                    //默认值跳过序列化
                    continue;
                }
                
                AppendMember(item.Value.FieldType,item.Value.Name,fieldValue,depth + 1);
                needRemoveLastComma = true;
            }
            
            if (needRemoveLastComma)
            {
                //删除末尾多出来的逗号
                
                //需要删除的字符长度
                int needRemoveLength = 1;
                if (JsonParser.IsFormat)
                {
                    //开启了格式化序列化，需要额外删除一个换行符
                    needRemoveLength += TextUtil.NewLineLength;
                }

                //最后一个逗号的位置
                int lastCommaIndex = TextUtil.CachedSB.Length - needRemoveLength;
                
                TextUtil.CachedSB.Remove(lastCommaIndex, needRemoveLength);
            }
            
            TextUtil.AppendLine(string.Empty);
            TextUtil.Append("}",depth);
        }

        /// <inheritdoc />
        public object ParseJson(Type type, Type realType)
        {
            if (!propertyInfoDict.ContainsKey(realType) && !fieldInfoDict.ContainsKey(realType))
            {
                //初始化反射信息
                AddReflectionInfo(realType);
            }
            
            object obj = TypeUtil.CreateInstance(realType);
            
            ParserHelper.ParseJsonObjectProcedure(obj, realType, default, (userdata1, userdata2, _, key) =>
            {
                Type t = (Type) userdata2;
                if (propertyInfoDict[t].TryGetValue(key, out PropertyInfo pi))
                {
                    //先尝试获取名为key的属性信息
                    object value = JsonParser.InternalParseJson(pi.PropertyType);
                    pi.SetValue(userdata1, value);
                }
                else if (fieldInfoDict[t].TryGetValue(key, out FieldInfo fi))
                {
                    //属性没有 再试试字段
                    object value = JsonParser.InternalParseJson(fi.FieldType);
                    fi.SetValue(userdata1, value);
                }
                else
                {
                    //这个json key既不是数据类的字段也不是属性，跳过
                    JsonParser.InternalParseJson<JsonValue>();
                }
            });

            return obj;
        }

        /// <summary>
        /// 添加反射信息到字典中
        /// </summary>
        private void AddReflectionInfo(Type type)
        {
            PropertyInfo[] pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<RangeString, PropertyInfo>  piDict = new Dictionary<RangeString, PropertyInfo>(pis.Length);
            for (int i = 0; i < pis.Length; i++)
            {
                PropertyInfo pi = pis[i];

                if (IsIgnore(pi,type,pi.Name))
                {
                    //需要忽略
                    continue;
                }
                
                if (pi.SetMethod != null && pi.GetMethod != null && pi.Name != "Item")
                {
                    //属性必须同时具有get set 并且不能是索引器item
                    piDict.Add(new RangeString(pi.Name), pi);
                }
                    
            }
            propertyInfoDict.Add(type, piDict);

            FieldInfo[] fis = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<RangeString, FieldInfo> fiDict = new Dictionary<RangeString, FieldInfo>(fis.Length);
            for (int i = 0; i < fis.Length; i++)
            {
                FieldInfo fi = fis[i];
                
                if (IsIgnore(fi,type,fi.Name))
                {
                    //需要忽略
                    continue;
                }
                
                fiDict.Add(new RangeString(fi.Name), fi);
            }
            fieldInfoDict.Add(type, fiDict);
        }

        /// <summary>
        /// 是否需要忽略此字段/属性
        /// </summary>
        private bool IsIgnore(MemberInfo mi,Type type,string name)
        {
            if (Attribute.IsDefined(mi, typeof(JsonIgnoreAttribute)))
            {
                return true;
            }

            if (ignoreSet.TryGetValue(type, out HashSet<string> set) && set.Contains(name))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 添加需要忽略的成员
        /// </summary>
        public void AddIgnoreMember(Type type,string memberName)
        {
            if (!ignoreSet.TryGetValue(type,out HashSet<string> set))
            {
                set = new HashSet<string>();
                ignoreSet.Add(type,set);
            }

            set.Add(memberName);
        }
        
        /// <summary>
        /// 追加字段/属性的json文本
        /// </summary>
        private static void AppendMember(Type memberType,string memberName,object value,int depth)
        {
            TextUtil.Append("\"", depth);
            TextUtil.Append(memberName);
            TextUtil.Append("\"");
            TextUtil.Append(":");
        
            JsonParser.InternalToJson(value,memberType,null,depth + 1);

            TextUtil.AppendLine(",");
        }
    }
}