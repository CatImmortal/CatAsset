using System.Collections;
using System.Collections.Generic;
using System;

namespace CatJson
{
    /// <summary>
    /// 字典类型的Json格式化器
    /// </summary>
    public class DictionaryFormatter : BaseJsonFormatter<IDictionary>
    {

        /// <inheritdoc />
        public override void ToJson(IDictionary value, Type type, Type realType, int depth)
        {
            Type dictType = type;
            if (!type.IsGenericType)
            {
                //此处的处理原因类似ArrayFormatter
                dictType = realType;
            }
            Type valueType = TypeUtil.GetDictValueType(dictType);

            TextUtil.AppendLine("{");

            if (value != null)
            {
                int index = 0;
                foreach (DictionaryEntry item in value)
                {
                    
                    TextUtil.Append("\"", depth);
                    TextUtil.Append(item.Key.ToString());
                    TextUtil.Append("\"");

                    TextUtil.Append(":");

                    JsonParser.InternalToJson(item.Value,valueType,null,depth + 1);

                    if (index < value.Count-1)
                    {
                        TextUtil.AppendLine(",");
                    }
                    index++;
                }
            }

            TextUtil.AppendLine(string.Empty);
            TextUtil.Append("}", depth - 1);
        }

        /// <inheritdoc />
        public override IDictionary ParseJson(Type type, Type realType)
        {
            IDictionary dict = (IDictionary)TypeUtil.CreateInstance(realType);
            Type dictType = type;
            if (!type.IsGenericType)
            {
                dictType = realType;
            }
            Type keyType = dictType.GetGenericArguments()[0];
            Type valueType = TypeUtil.GetDictValueType(dictType);
            
            ParserHelper.ParseJsonObjectProcedure(dict,valueType,TypeUtil.TypeEquals(keyType,typeof(int)), (userdata1,userdata2,isIntKey, key) =>
            {
                IDictionary localDict = (IDictionary) userdata1;
                Type localValueType = (Type) userdata2;

                object value = JsonParser.InternalParseJson(localValueType);
                if (!isIntKey)
                {
                    localDict.Add(key.ToString(), value);
                }
                else
                {
                    //处理字典key为int的情况
                    localDict.Add(int.Parse(key.AsSpan()), value);
                }
            });

            return dict;
        }
    }
}