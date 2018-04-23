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
        public void Field_query_must_parse_correctly()
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
        }

        [TestMethod]
        public void Term_query_must_parse_correctly()
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

            query = "qu?ck bro*";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 2);
            Assert.IsTrue(qs.ToString() == query);

            query = "quick OR brown AND fox AND NOT news";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 8);
            Assert.IsTrue(qs.ToString() == query);

            query = "(quick OR brown) AND fox";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 3);
            Assert.IsTrue(qs.ToString() == query);

            query = "status:(active OR pending) title:(full text search)^2";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 3);
            Assert.IsTrue(qs.ToString() == query);

            query = "(status:(active OR pending) AND title:(full text search)^2) OR (status:inactive AND title:deleted)";
            qs = query.ToQueryString();
            Assert.IsTrue(qs.Tokens.Length == 3);
            Assert.IsTrue(qs.ToString() == query);
        }

        [TestMethod]
        public void String_query_must_return_correct_terms()
        {
            string query = "quick brown +fox -news";
            var terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 4);
            Assert.IsTrue(terms[0] == "quick");
            Assert.IsTrue(terms[2] == "fox");

            query = "\"john smith\" +fox -news";
            terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 3);
            Assert.IsTrue(terms[0] == "\"john smith\"");
            Assert.IsTrue(terms[2] == "news");

            query = "quikc~ brwn~2 foks~ \"fox quick\"~5";
            terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 4);
            Assert.IsTrue(terms[0] == "quikc");
            Assert.IsTrue(terms[2] == "foks");

            query = "qu?ck bro*";
            terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 2);
            Assert.IsTrue(terms[0] == "qu?ck");
            Assert.IsTrue(terms[1] == "bro*");

            query = "quick OR brown AND fox AND NOT news";
            terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 4);
            Assert.IsTrue(terms[0] == "quick");
            Assert.IsTrue(terms[2] == "fox");

            query = "(quick OR brown) AND fox";
            terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 3);
            Assert.IsTrue(terms[0] == "quick");
            Assert.IsTrue(terms[2] == "fox");

            query = "status:(active OR pending) title:(full text search)^2";
            terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 5);
            Assert.IsTrue(terms[0] == "active");
            Assert.IsTrue(terms[2] == "full");

            query = "(status:(active OR pending) AND title:(full text search)^2) OR (status:(active OR pending) AND title:(full text search)^2)";
            terms = query.ToQueryString().GetTerms();
            Assert.IsTrue(terms.Length == 5);
            Assert.IsTrue(terms[0] == "active");
            Assert.IsTrue(terms[2] == "full");
        }
    }
}