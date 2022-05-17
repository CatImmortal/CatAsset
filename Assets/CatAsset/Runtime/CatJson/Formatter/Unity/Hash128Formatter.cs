using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Hash128类型的Json格式化器
    /// </summary>
    public class Hash128Formatter : BaseJsonFormatter<Hash128>
    {
        /// <inheritdoc />
        public override void ToJson(Hash128 value, Type type, Type realType, int depth)
        {
            TextUtil.Append($"\"{value.ToString()}\"");
        }

        /// <inheritdoc />
        public override Hash128 ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.String);
            return Hash128.Parse(rs.ToString());
        }
    }
}