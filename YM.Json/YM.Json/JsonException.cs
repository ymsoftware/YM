using System;

namespace YM.Json
{
    public class JsonException : Exception
    {
        public JsonException(string message)
            : base(message)
        {

        }

        public JsonException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }
}
