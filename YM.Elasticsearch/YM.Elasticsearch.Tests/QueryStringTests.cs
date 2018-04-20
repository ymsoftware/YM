using Microsoft.VisualStudio.TestTools.UnitTesting;
using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Elasticsearch.Query.FullTextQueries.QueryString;
using YM.Elasticsearch.Query.TermQueries;

namespace YM.Elasticsearch.Tests
{
    [TestClass]
    public class QueryStringTests
    {
        [TestMethod]
        public void field_query()
        {
            string query = "status:active";
            var qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            var token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is MatchQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "book.\\*:quick";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is MatchQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "_exists_:title";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is MatchQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "author:\"John Smith\"";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is MatchQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "book.\\*:(quick AND brown)";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is MatchQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "name:/joh?n(ath[oa]n)/";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is MatchQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "age:>10";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is RangeQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "age:<=10";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is RangeQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "date:[2012-01-01 TO 2012-12-31]";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is RangeQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "tag:{alpha TO omega}";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is RangeQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "date:{* TO 2012-01-01]";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 1);
            token = qs.Tokens[0];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is RangeQuery);
            Assert.IsTrue(qs.ToString() == query);

            query = "+date:{* TO 2012-01-01] -age:<=10";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 4);
            token = qs.Tokens[1];
            Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            Assert.IsTrue(((QueryStringQueryToken)token).Query is RangeQuery);
            Assert.IsTrue(qs.ToString() == query);

            //query = "status: active";
            //qs = query.ToQueryString();
            //Assert.IsTrue(qs.Tokens.Length == 1);
            //token = qs.Tokens[0];
            //Assert.IsTrue(token.Type == QueryStringTokenType.Query);
            //Assert.IsTrue(((QueryStringQueryToken)token).Query is MatchQuery);
            //Assert.IsTrue(qs.ToString() == "active");
        }

        [TestMethod]
        public void term_query()
        {
            string query = "quick brown +fox -news";
            var qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 6);
            Assert.IsTrue(qs.ToString() == query);

            query = "\"john smith\" +fox -news";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 5);
            Assert.IsTrue(qs.ToString() == query);

            query = "quikc~ brwn~2 foks~ \"fox quick\"~5";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 8);
            Assert.IsTrue(qs.ToString() == query);
        }
    }
}