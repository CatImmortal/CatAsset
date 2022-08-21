namespace CatJson
{
    /// <summary>
    /// CatJson扩展方法类
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 将此对象序列化为Json文本
        /// </summary>
        public static string ToJson<T>(this T self)
        {
            string json = JsonParser.ToJson(self);
            return json;
        }

        /// <summary>
        /// 将Json文本反序列化为指定类型的对象
        /// </summary>
        public static T ParseJson<T>(this string self)
        {
            T result = JsonParser.ParseJson<T>(self);
            return result;
        }
    }
}