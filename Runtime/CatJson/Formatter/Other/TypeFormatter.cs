using System;

namespace CatJson
{
    /// <summary>
    /// Type类型的Json格式化器
    /// </summary>
    public class TypeFormatter : BaseJsonFormatter<Type>
    {
        public override void ToJson(Type value, Type type, Type realType, int depth)
        {
            TextUtil.Append(TypeUtil.GetTypeString(value));
        }

        public override Type ParseJson(Type type, Type realType)
        {
            RangeString rs = JsonParser.Lexer.GetNextTokenByType(TokenType.String);
            string typeStr = rs.ToString();
           
#if FUCK_LUA
            //type字符串未包含逗号，意味着这是个热更层Type
            if (!typeStr.Contains(','))
            {
                return AppDomain.GetType(typeStr).ReflectionType;
            }
#endif
            return Type.GetType(typeStr);
        }
        

    }
}