using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Color类型的Json格式化器
    /// </summary>
    public class ColorFormatter : BaseJsonFormatter<Color>
    {
        /// <inheritdoc />
        public override void ToJson(Color value, Type type, Type realType, int depth)
        {

            TextUtil.Append('{');
            TextUtil.Append(value.r.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.g.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.b.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.a.ToString());
            TextUtil.Append('}');
        }

        /// <inheritdoc />
        public override Color ParseJson(Type type, Type realType)
        {
            JsonParser.Lexer.GetNextTokenByType(TokenType.LeftBrace);
            float r = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float g = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float b = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float a = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.RightBrace);
            return new Color(r, g, b, a);
        }
    }
}