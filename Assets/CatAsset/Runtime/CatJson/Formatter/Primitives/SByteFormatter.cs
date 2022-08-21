using System;

namespace CatJson
{
    /// <summary>
    /// sbyte类型的Json格式化器
    /// </summary>
    public class SByteFormatter : BaseJsonFormatter<sbyte>
    {
        /// <inheritdoc />
        public override void ToJson(sbyte value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        /// <inheritdoc />
        public override sbyte ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return sbyte.Parse(rs.AsSpan());
        }
    }
}