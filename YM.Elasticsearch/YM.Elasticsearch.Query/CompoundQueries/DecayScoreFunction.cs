using YM.Json;

namespace YM.Elasticsearch.Query.CompoundQueries
{
    public class DecayScoreFunction : QueryBase, IScoreFunction
    {
        public DecayFunction Function { get; private set; }
        public string Field { get; private set; }
        public string Origin { get; private set; }
        public string Scale { get; private set; }
        public string Offset { get; private set; }
        public double Decay { get; private set; }

        public DecayScoreFunction(DecayFunction function, string field, string origin, string scale, string offset = null, double decay = 0)
        {
            Function = function;
            Field = field;
            Origin = origin;
            Scale = scale;
            Offset = offset;
            Decay = decay;
        }

        public override QueryType Type => QueryType.DecayScoreFunction;

        public override JsonObject ToJson()
        {
            var field = new JsonObject()
                .Add("origin", Origin)
                .Add("scale", Scale);

            if (!string.IsNullOrWhiteSpace(Offset))
            {
                field.Add("offset", Offset);
            }

            if (Decay > 0)
            {
                field.Add("decay", Decay);
            }

            return new JsonObject().Add(Function.ToString().ToLower(), new JsonObject().Add(Field, field));
        }
    }
}
