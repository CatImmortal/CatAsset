using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Bounds类型的Json格式化器
    /// </summary>
    public class BoundsFormatter : BaseJsonFormatter<Bounds>
    {
        /// <inheritdoc />
        public override void ToJson(Bounds value, Type type, Type realType, int depth)
        {
            TextUtil.Append('{');
            
            TextUtil.Append(value.center.x.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.center.y.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.center.z.ToString());
            TextUtil.Append(",  ");
            
            TextUtil.Append(value.size.x.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.size.y.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.size.z.ToString());

            TextUtil.Append('}');
        }

        /// <inheritdoc />
        public override Bounds ParseJson(Type type, Type realType)
        {
            JsonParser.Lexer.GetNextTokenByType(TokenType.LeftBrace);
            
            float centerX = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float centerY = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float centerZ = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            Vector3 center = new Vector3(centerX, centerY, centerZ);
            
            float sizeX = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float sizeY = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float sizeZ = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.RightBrace);
            Vector3 size = new Vector3(sizeX, sizeY, sizeZ);

            return new Bounds(center, size);
        }
    }
}