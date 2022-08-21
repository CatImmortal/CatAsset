using System;
using System.Collections;
using System.Collections.Generic;

namespace CatJson
{
    /// <summary>
    /// 数组类型的Json格式化器
    /// </summary>
    public class ArrayFormatter : BaseJsonFormatter<Array>
    {
        /// <inheritdoc />
        public override void ToJson(Array value, Type type, Type realType, int depth)
        {
            TextUtil.AppendLine("[");
            Type arrayType = type;
            if (!type.IsArray)
            {
                //type可能是object,这时需要用realType作为arrayType
                //但又不能一开始就把realType当arrayType，因为value可能是热更层数组，这样realType会是ILTypeInstance[]，会导致后续获取到的元素类型为ILTypeInstance
                //因此这里有限制，无法多态序列化热更层的定义类型为object的任意数组字段/属性，因为真实类型信息根本没有
                arrayType = realType;
            }
            Type elementType = TypeUtil.GetArrayOrListElementType(arrayType);
            for (int i = 0; i < value.Length; i++)
            {
                object element = value.GetValue(i);
                TextUtil.AppendTab(depth);
                if (element == null)
                {
                    TextUtil.Append("null");
                }
                else
                {
                    JsonParser.InternalToJson(element,elementType,null,depth + 1);
                }
                if (i < value.Length-1)
                {
                    TextUtil.AppendLine(",");
                }
                 
            }
            TextUtil.AppendLine(string.Empty);
            TextUtil.Append("]", depth - 1);
        }

        /// <inheritdoc />
        public override Array ParseJson(Type type, Type realType)
        {
            List<object> list = new List<object>();
            Type arrayType = type;
            if (!type.IsArray)
            {
                arrayType = realType;
            }
            Type elementType = TypeUtil.GetArrayOrListElementType(arrayType);
            
            ParserHelper.ParseJsonArrayProcedure(list, elementType, (userdata1, userdata2) =>
            {
                IList localList = (IList) userdata1;
                Type localElementType = (Type) userdata2;
                object value = JsonParser.InternalParseJson(localElementType);
                localList.Add(value);
            });
            
            Array array = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                object element = list[i];
                array.SetValue(element, i);
            }

            return array;
        }
    }
}