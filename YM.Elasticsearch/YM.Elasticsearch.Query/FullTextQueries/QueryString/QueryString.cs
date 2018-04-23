using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryString
    {
        public QueryStringToken[] Tokens { get; private set; }

        public QueryString(QueryStringToken[] tokens)
        {
            Tokens = tokens;
        }

        public bool IsEmpty
        {
            get
            {
                return Tokens == null || Tokens.Length == 0;
            }
        }

        public string[] GetTerms()
        {
            if (IsEmpty)
            {
                return null;
            }

            var terms = new List<string>();

            foreach (var token in Tokens)
            {
                if (token.Type == QueryStringTokenType.Term)
                {
                    terms.Add(((QueryStringTermToken)token).Term);
                }
                else if (token.Type == QueryStringTokenType.Query)
                {
                    var query = ((QueryStringQueryToken)token).Query;
                    if (query is MatchQuery)
                    {
                        var values = ((MatchQuery)query)
                            .Value
                            .ToString()
                            .Split(new char[] {
                                QueryStringParser.TOKEN_SPACE,
                                QueryStringParser.TOKEN_LEFT_PAREN,
                                QueryStringParser.TOKEN_RIGHT_PAREN,
                                QueryStringParser.TOKEN_PLUS,
                                QueryStringParser.TOKEN_MINUS
                            })
                            .Where(e => !string.IsNullOrWhiteSpace(e)
                                && e != "AND"
                                && e != "OR"
                                && e != "NOT")
                            .ToArray();

                        terms.AddRange(values);
                    }
                }
                else if (token.Type == QueryStringTokenType.Group)
                {
                    var group = ((QueryStringGroupToken)token).Group;
                    terms.AddRange(group.GetTerms());
                }
            }

            return terms.Distinct().ToArray();
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return "";
            }

            var sb = new StringBuilder();

            QueryStringToken prev = null;

            foreach (var token in Tokens)
            {
                bool nospace = prev == null 
                    || prev.Type == QueryStringTokenType.Must 
                    || prev.Type == QueryStringTokenType.MustNot
                    || token.Type == QueryStringTokenType.Fuzzy
                    || token.Type == QueryStringTokenType.Boost;

                if (!nospace)
                {
                    sb.Append(" ");
                }

                sb.Append(token.ToString());

                prev = token;
            }

            return sb.ToString();
        }
    }
}