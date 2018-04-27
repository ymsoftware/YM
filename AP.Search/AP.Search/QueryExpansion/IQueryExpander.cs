using YM.Elasticsearch.Query;

namespace AP.Search.QueryExpansion
{
    public interface IQueryExpander
    {
        IQuery Expand(IQuery query);
    }
}
