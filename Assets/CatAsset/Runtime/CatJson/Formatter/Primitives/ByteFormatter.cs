using System;

namespace CatJson
{
    /// <summary>
    /// byte类型的Json格式化器
    /// </summary>
    public class ByteFormatter : BaseJsonFormatter<byte>
    {
        public override void ToJson(byte value, Type type, Type realType, int depth)
        {
            TextUtil.Append(value.ToString());
        }

        public override byte ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.Number);
            return byte.Parse(rs.AsSpan());
        }
    }
}