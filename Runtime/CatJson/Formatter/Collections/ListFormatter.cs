using System;
using System.Collections;
using System.Collections.Generic;

namespace CatJson
{
    /// <summary>
    /// List类型的Json格式化器
    /// </summary>
    public class ListFormatter : BaseJsonFormatter<IList>
    {
        /// <inheritdoc />
        public override void ToJson(IList value, Type type, Type realType, int depth)
        {
            TextUtil.AppendLine("[");
            Type listType = type;
            if (!listType.IsGenericType)
            {
                //此处的处理原因类似ArrayFormatter
                listType = realType;
            }
            Type elementType = TypeUtil.GetArrayOrListElementType(listType);
            for (int i = 0; i < value.Count; i++)
            {
                object element = value[i];
                TextUtil.AppendTab(depth);
                if (element == null)
                {
                    TextUtil.Append("null");
                }
                else
                {
                    JsonParser.InternalToJson(element,elementType,null,depth + 1);
                }
                if (i < value.Count - 1)
                {
                    TextUtil.AppendLine(",");
                }
                 
            }
            TextUtil.AppendLine(string.Empty);
            TextUtil.Append("]", depth - 1);
        }

        /// <inheritdoc />
        public override IList ParseJson(Type type, Type realType)
        {
            IList list = (IList)TypeUtil.CreateInstance(realType);
            
            Type listType = type;
            if (!listType.IsGenericType)
            {
                listType = realType;
            }
            Type elementType = TypeUtil.GetArrayOrListElementType(listType);
            
            ParserHelper.ParseJsonArrayProcedure(list, elementType, (userdata1, userdata2) =>
            {
                IList localList = (IList) userdata1;
                Type localElementType = (Type) userdata2;
                
                object value = JsonParser.InternalParseJson(localElementType);
                localList.Add(value);
            });

            return list;
        }
    }
}