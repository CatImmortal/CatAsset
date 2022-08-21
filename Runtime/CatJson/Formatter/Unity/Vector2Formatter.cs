using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Vector2类型的Json格式化器
    /// </summary>
    public class Vector2Formatter : BaseJsonFormatter<Vector2>
    {
        /// <inheritdoc />
        public override void ToJson(Vector2 value, Type type, Type realType, int depth)
        {
            TextUtil.Append('{');
            TextUtil.Append(value.x.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.y.ToString());
            TextUtil.Append('}');
        }

        /// <inheritdoc />
        public override Vector2 ParseJson(Type type, Type realType)
        {
            JsonParser.Lexer.GetNextTokenByType(TokenType.LeftBrace);
            float x = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float y = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.RightBrace);
            return new Vector2(x,y);
        }
    }
}