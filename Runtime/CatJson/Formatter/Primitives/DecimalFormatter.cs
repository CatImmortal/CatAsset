using System;

namespace CatJson
{
    /// <summary>
    /// decimal类型的Json格式化器
    /// </summary>
    public class DecimalFormatter : BaseJsonFormatter<decimal>
    {
        /// <inheritdoc />
        public override void ToJson(decimal value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        /// <inheritdoc />
        public override decimal ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return decimal.Parse(rs.AsSpan());
        }
    }
}