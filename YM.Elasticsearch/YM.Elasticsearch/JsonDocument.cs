using YM.Json;

namespace YM.Elasticsearch
{
    public abstract class JsonDocument
    {
        public abstract JsonObject ToJson();

        public override string ToString()
        {
            return ToJson().ToString(true);
        }

    }
}
