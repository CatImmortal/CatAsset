using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Vector3类型的Json格式化器
    /// </summary>
    public class Vector3Formatter : BaseJsonFormatter<Vector3>
    {
        /// <inheritdoc />
        public override void ToJson(Vector3 value, Type type, Type realType, int depth)
        {
            TextUtil.Append('{');
            TextUtil.Append(value.x.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.y.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.z.ToString());
            TextUtil.Append('}');
        }

        /// <inheritdoc />
        public override Vector3 ParseJson(Type type, Type realType)
        {
            JsonParser.Lexer.GetNextTokenByType(TokenType.LeftBrace);
            float x = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float y = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float z = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.RightBrace);
            return new Vector3(x,y,z);
        }
    }
}