using System;
using System.Collections.Generic;
using System.Reflection;

namespace CatJson
{
    /// <summary>
    /// 基于反射的json格式化器，会通过反射来处理字段/属性
    /// </summary>
    public class ReflectionFormatter : IJsonFormatter
    {
        /// <inheritdoc />
        public void ToJson(object value, Type type, Type realType, int depth)
        {
            //是否需要删除最后一个逗号
            bool needRemoveLastComma = false;
            
            TextUtil.AppendLine("{");
            
            //序列化字段
            Dictionary<RangeString, FieldInfo> fieldInfos = TypeMetaDataManager.GetFieldInfos(realType);
            foreach (KeyValuePair<RangeString, FieldInfo> item in fieldInfos)
            {
                object fieldValue = item.Value.GetValue(value);

                if (IsJump(realType,fieldValue))
                {
                    //跳过默认值序列化
                    continue;
                }
                
                AppendMember(item.Value.FieldType,item.Value.Name,fieldValue,depth);
                needRemoveLastComma = true;
            }
            //序列化属性
            Dictionary<RangeString, PropertyInfo> propertyInfos = TypeMetaDataManager.GetPropertyInfos(realType);
            foreach (KeyValuePair<RangeString, PropertyInfo> item in propertyInfos)
            {
                object propValue = item.Value.GetValue(value);
                
                if (IsJump(realType,propValue))
                {
                    //跳过默认值序列化
                    continue;
                }

                AppendMember(item.Value.PropertyType,item.Value.Name,propValue,depth);
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
            TextUtil.Append("}",depth - 1);
        }

        /// <inheritdoc />
        public object ParseJson(Type type, Type realType)
        {
           
            object obj = TypeUtil.CreateInstance(realType);
            
            ParserHelper.ParseJsonObjectProcedure(obj, realType, default, (userdata1, userdata2, _, key) =>
            {
                object localObj = userdata1;
                Type localRealType = (Type) userdata2;
                Dictionary<RangeString, FieldInfo> fieldInfos = TypeMetaDataManager.GetFieldInfos(localRealType);
                Dictionary<RangeString, PropertyInfo> propertyInfos = TypeMetaDataManager.GetPropertyInfos(localRealType);

                if (fieldInfos.TryGetValue(key, out FieldInfo fi))
                {
                    //是字段
                    object value = JsonParser.InternalParseJson(fi.FieldType);
                    fi.SetValue(localObj, value);
                }
                else if (propertyInfos.TryGetValue(key, out PropertyInfo pi))
                {
                    //是属性
                    object value = JsonParser.InternalParseJson(pi.PropertyType);
                    pi.SetValue(localObj, value);
                }
                else
                {
                    //这个json key既不是数据类的字段也不是属性，跳过
                    JsonParser.ParseJson<JsonValue>();
                }


            });

            return obj;
        }

        /// <summary>
        /// 是否跳过处理
        /// </summary>
        private bool IsJump(Type realType,object value)
        {
            if (JsonParser.IgnoreDefaultValue && !TypeMetaDataManager.IsCareDefaultValue(realType))
            {
                if (TypeUtil.IsDefaultValue(value))
                {
                    //跳过默认值序列化
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// 追加字段/属性的json文本
        /// </summary>
        private static void AppendMember(Type memberType,string memberName,object value,int depth)
        {
            //key
            TextUtil.Append("\"", depth);
            TextUtil.Append(memberName);
            TextUtil.Append("\"");
            
            TextUtil.Append(":");
        
            //value
            JsonParser.InternalToJson(value,memberType,null,depth + 1);

            TextUtil.AppendLine(",");
        }
    }
}