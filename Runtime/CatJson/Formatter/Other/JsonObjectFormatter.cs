using System;
using System.Collections.Generic;

namespace CatJson
{
    /// <summary>
    /// JsonObject类型的Json格式化器
    /// </summary>
    public class JsonObjectFormatter : BaseJsonFormatter<JsonObject>
    {
        /// <inheritdoc />
        public override void ToJson(JsonObject value, Type type, Type realType, int depth)
        {
            TextUtil.AppendLine("{");

            if (value.ValueDict != null)
            {
                int index = 0;
                foreach (KeyValuePair<string, JsonValue> item in value.ValueDict)
                {
                    
                    TextUtil.Append("\"", depth);
                    TextUtil.Append(item.Key);
                    TextUtil.Append("\"");

                    TextUtil.Append(":");

                    JsonParser.ToJson(item.Value,depth + 1);

                    if (index < value.ValueDict.Count-1)
                    {
                        TextUtil.AppendLine(",");
                    }
                    index++;
                }
            }

            TextUtil.AppendLine(string.Empty);
            TextUtil.Append("}", depth - 1);
        }

        /// <inheritdoc />
        public override JsonObject ParseJson(Type type, Type realType)
        {
            JsonObject obj = new JsonObject();

            ParserHelper.ParseJsonObjectProcedure(obj, default, default, (userdata1, _, _, key) =>
            {
                JsonObject localObj = (JsonObject) userdata1;
                JsonValue value = JsonParser.ParseJson<JsonValue>();
                localObj[key.ToString()] = value;
            });

            return obj;
        }


    }
}