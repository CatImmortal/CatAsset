using System;

namespace CatJson
{
    /// <summary>
    /// char类型的Json格式化器
    /// </summary>
    public class CharFormatter : BaseJsonFormatter<char>
    {
        /// <inheritdoc />
        public override void ToJson(char value, Type type, Type realType, int depth)
        {
            TextUtil.Append('\"');
            TextUtil.Append(value);
            TextUtil.Append('\"');
        }

        /// <inheritdoc />
        public override char ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.String);
            return rs[0];
        }
    }
}