using System;
using UnityEngine;

namespace CatJson
{
    /// <summary>
    /// 字符串类型的Json格式化器
    /// </summary>
    public class StringFormatter : BaseJsonFormatter<string>
    {
        /// <inheritdoc />
        public override void ToJson(string value, Type type, Type realType, int depth)
        {
            //TextUtil.Append($"\"{value}\"");
            
            TextUtil.Append("\"");
            for (int i = 0; i < value.Length; i++)
            {
               TextUtil.CachedSB.Append(value[i]);

               if (value[i] == '\\')
               {
                   //特殊处理包含\字符的情况，要额外多写入一个\，这样在ParseJson的时候才能被正确解析
                   TextUtil.CachedSB.Append('\\');
               }
            }
            TextUtil.Append("\"");
            
          
        }

        /// <inheritdoc />
        public override string ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.String);
            return rs.ToString();
        }
    }
}