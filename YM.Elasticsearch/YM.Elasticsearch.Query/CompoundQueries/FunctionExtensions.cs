using YM.Json;

namespace YM.Elasticsearch.Query.CompoundQueries
{
    static class FunctionExtensions
    {
        public static IScoreFunction ToFunction(this JsonObject jo)
        {
            if (jo == null || jo.IsEmpty)
            {
                return null;
            }

            foreach(var jp in jo.Properties())
            {
                switch (jp.Name)
                {
                    case "gauss": return ParseDecayScoreFunction(DecayFunction.Gauss, jp.Value.Get<JsonObject>());
                    case "linear": return ParseDecayScoreFunction(DecayFunction.Linear, jp.Value.Get<JsonObject>());
                    case "exp": return ParseDecayScoreFunction(DecayFunction.Exp, jp.Value.Get<JsonObject>());
                    case "filter": return ParseFilterScoreFunction(jp.Value.Get<JsonObject>(), jo.Property<double>("weight"));
                }
            }           

            return null;
        }

        private static DecayScoreFunction ParseDecayScoreFunction(DecayFunction function, JsonObject jo)
        {
            var jp = jo.Properties()[0];
            var value = jp.Value.Get<JsonObject>();
            if (value == null)
            {
                return null;
            }

            string origin = null;
            string scale = null;
            string offset = null;
            double decay = 0;

            foreach (var p in value.Properties())
            {
                switch (p.Name)
                {
                    case "origin": origin = p.Value.Get<string>(); break;
                    case "scale": scale = p.Value.Get<string>(); break;
                    case "offset": offset = p.Value.Get<string>(); break;
                    case "decay": decay = p.Value.Get<double>(); break;
                }
            }

            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(scale))
            {
                return null;
            }

            return new DecayScoreFunction(function, jp.Name, origin, scale, offset, decay);
        }

        private static FilterScoreFunction ParseFilterScoreFunction(JsonObject jo, double weight)
        {
            if (weight <= 0)
            {
                return null;
            }

            var query = jo.ToQuery();
            if (query == null)
            {
                return null;
            }

            return new FilterScoreFunction(query, weight);
        }
    }
}
