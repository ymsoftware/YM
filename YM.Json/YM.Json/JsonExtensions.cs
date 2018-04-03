namespace YM.Json
{
    public static class JsonExtensions
    {
        public static JsonValue ToJson(this string s)
        {
            var parser = new CharJsonParser();
            return parser.Parse(s);
        }
    }
}
