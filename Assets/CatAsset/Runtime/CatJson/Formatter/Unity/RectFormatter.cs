using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Rect类型的Json格式化器
    /// </summary>
    public class RectFormatter : BaseJsonFormatter<Rect>
    {
        /// <inheritdoc />
        public override void ToJson(Rect value, Type type, Type realType, int depth)
        {
            TextUtil.Append('{');
            TextUtil.Append(value.x.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.y.ToString());
            TextUtil.Append(",");
            TextUtil.Append(value.width.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.height.ToString());
            TextUtil.Append('}');
        }

        /// <inheritdoc />
        public override Rect ParseJson(Type type, Type realType)
        {
            JsonParser.Lexer.GetNextTokenByType(TokenType.LeftBrace);
            float x = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float y = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float width = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float height = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.RightBrace);
            
            return new Rect(x,y,width,height);
        }
    }
}