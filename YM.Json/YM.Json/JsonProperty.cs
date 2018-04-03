namespace YM.Json
{
    public class JsonProperty
    {
        public string Name { get; private set; }
        public JsonValue Value { get; private set; }

        public JsonProperty(string name, JsonValue value)
        {
            Name = name;
            Value = value;
        }

        public JsonProperty(string name, object value)
        {
            Name = name;
            Value = value is JsonProperty ? (value as JsonProperty).Value : new JsonValue(value);
        }

        public override string ToString()
        {
            return string.Format("\"{0}\": {1}", Name, Value);
        }
    }
}
