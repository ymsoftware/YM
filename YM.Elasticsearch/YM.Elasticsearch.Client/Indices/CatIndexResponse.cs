using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class CatIndexResponse : JsonDocument
    {
        public int StatusCode { get; private set; }
        public string Health { get; private set; }
        public string Status { get; private set; }
        public string Index { get; private set; }
        public int PrimaryShards { get; private set; }
        public int ReplicaShards { get; private set; }
        public int Documents { get; private set; }

        public CatIndexResponse(RestResponse response)
        {
            //yellow open alerts_20180411200429 VL4UtW9ZS2Sg6S6ibGxz2g 1 2 7782 0 12.2mb 12.2mb

            if (response.Http.IsSuccessStatusCode)
            {
                StatusCode = 200;

                var tokens = response.Content.Split(' ');

                Health = tokens[0];
                Status = tokens[1];
                Index = tokens[2];
                PrimaryShards = int.Parse(tokens[4]);
                ReplicaShards = int.Parse(tokens[5]);
                Documents = int.Parse(tokens[6]);
            }
            else
            {
                StatusCode = (int)response.Http.StatusCode;
                Status = response.Content;
            }
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("statusCode", StatusCode)
                .Add("status", Status);

            if (!string.IsNullOrWhiteSpace(Health)) jo.Add("health", Health);
            if (!string.IsNullOrWhiteSpace(Index)) jo.Add("index", Index);           
            if (PrimaryShards > 0) jo.Add("primary", PrimaryShards); jo.Add("replica", ReplicaShards);
            if (Documents > 0) jo.Add("documents", Documents);

            return jo;
        }
    }
}
