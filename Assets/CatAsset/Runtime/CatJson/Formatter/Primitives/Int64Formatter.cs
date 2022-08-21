using System;

namespace CatJson
{
    /// <summary>
    /// long类型的Json格式化器
    /// </summary>
    public class Int64Formatter : BaseJsonFormatter<long>
    {
        /// <inheritdoc />
        public override void ToJson(long value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        /// <inheritdoc />
        public override long ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return long.Parse(rs.AsSpan());
        }
    }
}
