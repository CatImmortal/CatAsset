using System;

namespace CatJson
{
    /// <summary>
    /// ushort类型的Json格式化器
    /// </summary>
    public class UInt16Formatter : BaseJsonFormatter<ushort>
    {
        /// <inheritdoc />
        public override void ToJson(ushort value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        /// <inheritdoc />
        public override ushort ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return ushort.Parse(rs.AsSpan());
        }
    }
}