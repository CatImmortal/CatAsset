using System;

namespace CatJson
{
    /// <summary>
    /// short类型的Json格式化器
    /// </summary>
    public class Int16Formatter : BaseJsonFormatter<short>
    {
        /// <inheritdoc />
        public override void ToJson(short value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        /// <inheritdoc />
        public override short ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return short.Parse(rs.AsSpan());
        }
    }
}