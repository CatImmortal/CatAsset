using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Vector4类型的Json格式化器
    /// </summary>
    public class Vector4Formatter : BaseJsonFormatter<Vector4>
    {
        /// <inheritdoc />
        public override void ToJson(Vector4 value, Type type, Type realType, int depth)
        {
            TextUtil.Append('{');
            TextUtil.Append(value.x.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.y.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.z.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.w.ToString());
            TextUtil.Append('}');
        }

        /// <inheritdoc />
        public override Vector4 ParseJson(Type type, Type realType)
        {
            JsonParser.Lexer.GetNextTokenByType(TokenType.LeftBrace);
            float x = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float y = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float z = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float w = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.RightBrace);
            return new Vector4(x,y,z,w);
        }
    }
}