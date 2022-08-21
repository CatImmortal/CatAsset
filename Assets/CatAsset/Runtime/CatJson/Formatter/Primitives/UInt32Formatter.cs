using System;

namespace CatJson
{
    /// <summary>
    /// uint类型的Json格式化器
    /// </summary>
    public class UInt32Formatter : BaseJsonFormatter<uint>
    {
        /// <inheritdoc />
        public override void ToJson(uint value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        /// <inheritdoc />
        public override uint ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return uint.Parse(rs.AsSpan());
        }
    }
}