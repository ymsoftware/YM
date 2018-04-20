using System;

namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryStringParseException : Exception
    {
        public QueryStringParseException(string message)
            : base(message)
        {

        }

        public QueryStringParseException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }
}
