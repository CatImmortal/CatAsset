using System;

namespace CatJson
{
    /// <summary>
    /// 枚举类型的Json格式化器
    /// </summary>
    public class EnumFormatter : IJsonFormatter
    {
        /// <inheritdoc />
        public void ToJson(object value, Type type, Type realType, int depth)
        {
            Type underlyingType = realType.GetEnumUnderlyingType();
            JsonParser.InternalToJson(value,underlyingType,underlyingType,depth,false);
        }

        /// <inheritdoc />
        public object ParseJson(Type type, Type realType)
        {
            Type underlyingType = realType.GetEnumUnderlyingType();
            object result = JsonParser.InternalParseJson(underlyingType, underlyingType, false);
            object obj = Enum.ToObject(realType, result);
            return obj;
        }
    }
}