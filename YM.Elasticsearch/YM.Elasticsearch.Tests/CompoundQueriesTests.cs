using Microsoft.VisualStudio.TestTools.UnitTesting;
using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Elasticsearch.Query.TermQueries;
using YM.Json;

namespace YM.Elasticsearch.Tests
{
    [TestClass]
    public class CompoundQueriesTests
    {
        [TestMethod]
        public void bool_query()
        {
            var query = new BoolQuery()
                .Must(new MatchQuery("headline", "Yuri Metelkin", true))
                .Filter(new TermQuery("type", "text"))
                .Filter(new TermsQuery<int>("products", new int[] { 1, 2, 3 }))
                .Not(new TermQuery("priority", 1))
                .Should(new MatchPhraseQuery("headline", "Yuri Metelkin"))
                .Should(new MatchPhraseQuery("title", "AP News"));

            Assert.IsTrue(query.MustQueries[0].Type == QueryType.MatchQuery);
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).Field == "headline");
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).IsAnd);
            Assert.IsTrue(query.FilterQueries[0].Type == QueryType.TermQuery);
            Assert.IsTrue(((TermQuery)query.FilterQueries[0]).Field == "type");
            Assert.IsTrue(((TermQuery)query.FilterQueries[0]).Value.ToString() == "text");
            Assert.IsTrue(query.FilterQueries[1].Type == QueryType.TermsQuery);
            Assert.IsTrue(((TermsQuery)query.FilterQueries[1]).Field == "products");
            Assert.IsTrue(((TermsQuery)query.FilterQueries[1]).Values[0].ToString() == "1");
            Assert.IsTrue(query.NotQueries[0].Type == QueryType.TermQuery);
            Assert.IsTrue(((TermQuery)query.NotQueries[0]).Field == "priority");
            Assert.IsTrue(((TermQuery)query.NotQueries[0]).Value.ToString() == "1");
            Assert.IsTrue(query.ShouldQueries[0].Type == QueryType.MatchPhraseQuery);
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[0]).Field == "headline");
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[0]).Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(query.ShouldQueries[1].Type == QueryType.MatchPhraseQuery);
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[1]).Field == "title");
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[1]).Value.ToString() == "AP News");

            string json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.BoolQuery);
            query = q as BoolQuery;
            Assert.IsTrue(query.MustQueries[0].Type == QueryType.MatchQuery);
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).Field == "headline");
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).IsAnd);
            Assert.IsTrue(query.FilterQueries[0].Type == QueryType.TermQuery);
            Assert.IsTrue(((TermQuery)query.FilterQueries[0]).Field == "type");
            Assert.IsTrue(((TermQuery)query.FilterQueries[0]).Value.ToString() == "text");
            Assert.IsTrue(query.FilterQueries[1].Type == QueryType.TermsQuery);
            Assert.IsTrue(((TermsQuery)query.FilterQueries[1]).Field == "products");
            Assert.IsTrue(((TermsQuery)query.FilterQueries[1]).Values[0].ToString() == "1");
            Assert.IsTrue(query.NotQueries[0].Type == QueryType.TermQuery);
            Assert.IsTrue(((TermQuery)query.NotQueries[0]).Field == "priority");
            Assert.IsTrue(((TermQuery)query.NotQueries[0]).Value.ToString() == "1");
            Assert.IsTrue(query.ShouldQueries[0].Type == QueryType.MatchPhraseQuery);
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[0]).Field == "headline");
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[0]).Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(query.ShouldQueries[1].Type == QueryType.MatchPhraseQuery);
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[1]).Field == "title");
            Assert.IsTrue(((MatchPhraseQuery)query.ShouldQueries[1]).Value.ToString() == "AP News");

            query = new BoolQuery()
                .Must(new MatchQuery("headline", "Yuri Metelkin", true));
            Assert.IsTrue(query.MustQueries[0].Type == QueryType.MatchQuery);
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).Field == "headline");
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(((MatchQuery)query.MustQueries[0]).IsAnd);

            json = query.ToString();
            jo = JsonObject.Parse(json);
            q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.MatchQuery);
            var match = q as MatchQuery;
            Assert.IsTrue(match.Field == "headline");
            Assert.IsTrue(match.Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(match.IsAnd);
        }

        [TestMethod]
        public void constant_score_query()
        {
            var query = new ConstantScoreQuery(new MatchQuery("headline", "Yuri Metelkin", true));

            Assert.IsTrue(query.Query.Type == QueryType.MatchQuery);
            Assert.IsTrue(((MatchQuery)query.Query).Field == "headline");
            Assert.IsTrue(((MatchQuery)query.Query).Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(((MatchQuery)query.Query).IsAnd);

            var json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.ConstantScoreQuery);
            var cs = q as ConstantScoreQuery;
            var match = cs.Query as MatchQuery;
            Assert.IsTrue(match.Field == "headline");
            Assert.IsTrue(match.Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(match.IsAnd);
        }

        [TestMethod]
        public void function_score_query()
        {
            var functions = new IScoreFunction[]
            {
                new FilterScoreFunction(new MatchQuery("type", "text"), 2),
                new DecayScoreFunction(DecayFunction.Gauss, "date", "2013-09-17", "10d", "5d", 0.5)
            };

            var query = new FunctionScoreQuery(new MatchQuery("headline", "Yuri Metelkin", true), functions);

            Assert.IsTrue(query.Query.Type == QueryType.MatchQuery);
            Assert.IsTrue(((MatchQuery)query.Query).Field == "headline");
            Assert.IsTrue(((MatchQuery)query.Query).Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(((MatchQuery)query.Query).IsAnd);

            var f = query.Functions[0];
            Assert.IsTrue(f.Type == QueryType.FilterScoreFunction);
            var fsf = f as FilterScoreFunction;
            Assert.IsTrue(fsf.Weight == 2);
            f = query.Functions[1];
            Assert.IsTrue(f.Type == QueryType.DecayScoreFunction);
            var dsf = f as DecayScoreFunction;
            Assert.IsTrue(dsf.Function == DecayFunction.Gauss);

            var json = query.ToString();
            var jo = JsonObject.Parse(json);
            var q = jo.ToQuery();
            Assert.IsTrue(q.Type == QueryType.FunctionScoreQuery);
            var fs = q as FunctionScoreQuery;
            var match = fs.Query as MatchQuery;
            Assert.IsTrue(match.Field == "headline");
            Assert.IsTrue(match.Value.ToString() == "Yuri Metelkin");
            Assert.IsTrue(match.IsAnd);

            f = query.Functions[0];
            Assert.IsTrue(f.Type == QueryType.FilterScoreFunction);
            fsf = f as FilterScoreFunction;
            Assert.IsTrue(fsf.Weight == 2);
            f = query.Functions[1];
            Assert.IsTrue(f.Type == QueryType.DecayScoreFunction);
            dsf = f as DecayScoreFunction;
            Assert.IsTrue(dsf.Function == DecayFunction.Gauss);
        }
    }
}