using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// Quaternion类型的Json格式化器
    /// </summary>
    public class QuaternionFormatter : BaseJsonFormatter<Quaternion>
    {
        /// <inheritdoc />
        public override void ToJson(Quaternion value, Type type, Type realType, int depth)
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
        public override Quaternion ParseJson(Type type, Type realType)
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
            return new Quaternion(x,y,z,w);
        }
    }
}