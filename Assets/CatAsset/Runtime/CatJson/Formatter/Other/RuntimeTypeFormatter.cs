using System;

namespace CatJson
{
    /// <summary>
    /// RuntimeType类型的Json格式化器
    /// </summary>
    public class RuntimeTypeFormatter : BaseJsonFormatter<Type>
    {
        /// <inheritdoc />
        public override void ToJson(Type value, Type type, Type realType, int depth)
        {
            TextUtil.Append(TypeUtil.GetTypeString(value));
        }

        /// <inheritdoc />
        public override Type ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.String);
            string typeStr = rs.ToString();
            return Type.GetType(typeStr);
        }
        

    }
}