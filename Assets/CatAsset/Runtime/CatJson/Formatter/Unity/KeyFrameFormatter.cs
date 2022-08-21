using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// KeyFrame类型的Json格式化器
    /// </summary>
    public class KeyFrameFormatter : BaseJsonFormatter<Keyframe>
    {
        /// <inheritdoc />
        public override void ToJson(Keyframe value, Type type, Type realType, int depth)
        {
            TextUtil.Append('{');
            TextUtil.Append(value.time.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.value.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.inTangent.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.outTangent.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.inWeight.ToString());
            TextUtil.Append(", ");
            TextUtil.Append(value.outWeight.ToString());
            TextUtil.Append('}');
        }

        /// <inheritdoc />
        public override Keyframe ParseJson(Type type, Type realType)
        {
            JsonParser.Lexer.GetNextTokenByType(TokenType.LeftBrace);
            float time = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float value = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float inTangent = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float outTangent = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float inWeight = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.Comma);
            float outWeight = JsonParser.Lexer.GetNextTokenByType(TokenType.Number).AsFloat();
            JsonParser.Lexer.GetNextTokenByType(TokenType.RightBrace);
            return new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight);
        }
    }
}