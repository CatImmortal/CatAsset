using System;

namespace CatJson
{
    /// <summary>
    /// ulong类型的Json格式化器
    /// </summary>
    public class UInt64Formatter : BaseJsonFormatter<ulong>
    {
        /// <inheritdoc />
        public override void ToJson(ulong value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        /// <inheritdoc />
        public override ulong ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return ulong.Parse(rs.AsSpan());
        }
    }
}
