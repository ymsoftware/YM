using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchSort : JsonDocument
    {
        public string Field { get; private set; }
        public bool IsDescending { get; private set; }
        public SortMode Mode { get; private set; }

        public SearchSort(string field, bool isDescending = false, SortMode mode = SortMode.None)
        {
            Field = field;
            IsDescending = isDescending;
            Mode = mode;
        }

        public SearchSort(JsonValue jv)
        {
            if (jv == null)
            {
                return;
            }

            if (jv.Type == JsonType.String)
            {
                Field = jv.Get<string>();
            }
            else if (jv.Type == JsonType.Object)
            {
                var jp = jv.Get<JsonObject>().Properties()[0];

                Field = jp.Name;

                var sort = jp.Value;

                if (sort.Type == JsonType.String)
                {
                    IsDescending = sort.Get<string>() == "desc";
                }
                else if (sort.Type == JsonType.Object)
                {
                    var jo = sort.Get<JsonObject>();

                    string order = jo.Property<string>("order");
                    IsDescending = !string.IsNullOrWhiteSpace(order) && order == "desc";

                    string mode = jo.Property<string>("mode");
                    switch (mode)
                    {
                        case "min": Mode = SortMode.Min; break;
                        case "max": Mode = SortMode.Max; break;
                        case "sum": Mode = SortMode.Sum; break;
                        case "avg": Mode = SortMode.Avg; break;
                        case "median": Mode = SortMode.Median; break;
                    }
                }
            }
        }

        public override JsonObject ToJson()
        {
            string order = IsDescending ? "desc" : "asc";

            if (Mode == SortMode.None)
            {
                return new JsonObject()
                    .Add(Field, order);
            }
            else
            {
                return new JsonObject()
                    .Add(Field, new JsonObject()
                        .Add("order", order)
                        .Add("mode", Mode.ToString().ToLower()));
            }
        }
    }
}