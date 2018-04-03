namespace YM.Json
{
    class JsonPathToken
    {
        public string Name { get; private set; }
        public bool IsArray { get; private set; }
        public int Index { get; private set; }

        public JsonPathToken(string name, bool array, int index = -1)
        {
            Name = name;
            IsArray = array;
            Index = index;
        }
    }
}
