namespace YM.Json
{
    public class JsonParseException : JsonException
    {
        public JsonParseException(string message)
            : base(message)
        {

        }

        public JsonParseException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }
}
