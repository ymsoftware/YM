using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Query.SpanQueries
{
    public class SpanNearQuery : QueryBase
    {
        public IQuery[] Clauses { get; private set; }
        public bool InOrder { get; private set; }
        public int? Slop { get; private set; }

        public override QueryType Type => QueryType.SpanNearQuery;

        public SpanNearQuery(IQuery[] clauses, bool inOrder, int? slop)
        {
            Clauses = clauses;
            InOrder = inOrder;
            Slop = slop;
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("clauses", Clauses.Select(e => e.ToJson()).ToArray())
                .Add("in_order", InOrder);

            if (Slop.HasValue)
            {
                jo.Add("slop", Slop.Value);
            }

            return new JsonObject().Add("span_near", jo);
        }
    }
}