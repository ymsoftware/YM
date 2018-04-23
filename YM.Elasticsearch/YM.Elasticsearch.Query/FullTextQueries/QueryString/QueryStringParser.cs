using System;
using System.Collections.Generic;
using YM.Elasticsearch.Query.TermQueries;

namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    class QueryStringParser
    {
        public const char TOKEN_SPACE = ' ';
        public const char TOKEN_PLUS = '+';
        public const char TOKEN_MINUS = '-';
        public const char TOKEN_COLON = ':';
        public const char TOKEN_PERIOD = '.';
        public const char TOKEN_BACKSLASH = '\\';
        public const char TOKEN_WILDCARD = '*';
        public const char TOKEN_UNDERSCORE = '_';
        public const char TOKEN_QUOTE = '"';
        public const char TOKEN_LEFT_PAREN = '(';
        public const char TOKEN_RIGHT_PAREN = ')';
        public const char TOKEN_LEFT_SQUARE = '[';
        public const char TOKEN_RIGHT_SQUARE = ']';
        public const char TOKEN_LEFT_CURLY = '{';
        public const char TOKEN_RIGHT_CURLY = '}';
        public const char TOKEN_GT = '>';
        public const char TOKEN_LT = '<';
        public const char TOKEN_EQ = '=';
        public const char TOKEN_T = 'T';
        public const char TOKEN_O = 'O';
        public const char TOKEN_BOOST = '^';
        public const char TOKEN_FUZZY = '~';
        public const char TOKEN_FORWARD_SLASH = '/';


        public const char TOKEN_NULL = '\0';
        public const char TOKEN_BL = '\a';
        public const char TOKEN_BS = '\b';
        public const char TOKEN_HT = '\t';
        public const char TOKEN_LF = '\n';
        public const char TOKEN_VT = '\v';
        public const char TOKEN_FF = '\f';
        public const char TOKEN_CR = '\r';

        
        public const char TOKEN_COMMA = ',';


        public const char TOKEN_0 = '0';
        public const char TOKEN_1 = '1';
        public const char TOKEN_2 = '2';
        public const char TOKEN_3 = '3';
        public const char TOKEN_4 = '4';
        public const char TOKEN_5 = '5';
        public const char TOKEN_6 = '6';
        public const char TOKEN_7 = '7';
        public const char TOKEN_8 = '8';
        public const char TOKEN_9 = '9';
       
        public const char TOKEN_A = 'a';
        public const char TOKEN_B = 'b';
        public const char TOKEN_E = 'e';
        public const char TOKEN_E_UPPER = 'E';
        public const char TOKEN_F = 'f';
        public const char TOKEN_L = 'l';
        public const char TOKEN_N = 'n';
        public const char TOKEN_R = 'r';
        public const char TOKEN_S = 's';
        
        public const char TOKEN_U = 'u';
        public const char TOKEN_V = 'v';               

        public QueryString Parse(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            var chars = query.ToCharArray();
            int size = chars.Length;
            int index = 0;

            return Parse(chars, size, ref index);
        }

        private QueryString Parse(char[] chars, int size, ref int index)
        {            
            var tokens = new List<QueryStringToken>();

            SkipWhitespace(chars, size, ref index);

            while (index < size)
            {
                char c = chars[index];

                switch (c)
                {
                    case TOKEN_LEFT_PAREN:
                        index++;
                        var qs = Parse(chars, size, ref index);
                        if (qs == null)
                        {
                            throw new QueryStringParseException("Expected group, did not find ')'");
                        }
                        tokens.Add(new QueryStringGroupToken(qs));
                        break;

                    case TOKEN_RIGHT_PAREN:
                        index++;
                        return new QueryString(tokens.ToArray());

                    case TOKEN_QUOTE:
                        string phrase = ParseString(chars, size, ref index);
                        tokens.Add(new QueryStringTermToken(phrase));
                        break;

                    case TOKEN_PLUS:
                        tokens.Add(ParseMust(chars, size, ref index));
                        break;

                    case TOKEN_MINUS:
                        tokens.Add(ParseMustNot(chars, size, ref index));
                        break;

                    case TOKEN_FUZZY:
                        tokens.Add(ParseFuzzy(chars, size, ref index));
                        break;

                    case TOKEN_BOOST:
                        tokens.Add(ParseBoost(chars, size, ref index));
                        break;

                    case TOKEN_SPACE:
                        break;

                    default:
                        tokens.Add(ParseText(chars, size, ref index));
                        break;

                }

                index++;
            }

            return new QueryString(tokens.ToArray());
        }

        private QueryStringToken ParseMust(char[] chars, int size, ref int index)
        {
            char c = chars[index + 1];

            if (Char.IsLetter(c))
            {
                return new QueryStringToken(QueryStringTokenType.Must);
            }

            throw new QueryStringParseException("Expected letter, found [{0}]", c);
        }

        private QueryStringToken ParseMustNot(char[] chars, int size, ref int index)
        {
            char c = chars[index + 1];

            if (Char.IsLetter(c))
            {
                return new QueryStringToken(QueryStringTokenType.MustNot);
            }

            throw new QueryStringParseException("Expected letter, found [{0}]", c);
        }

        private QueryStringToken ParseText(char[] chars, int size, ref int index)
        {
            int start = index;
            bool @break = false;

            while (index < size)
            {
                char c = chars[index];

                switch (c)
                {
                    case TOKEN_SPACE:
                    case TOKEN_RIGHT_PAREN:
                    case TOKEN_FUZZY:
                    case TOKEN_BOOST:
                        index--;
                        @break = true;
                        break;

                    case TOKEN_COLON:
                        return ParseQuery(start, chars, size, ref index);

                    case TOKEN_QUOTE:
                    case TOKEN_PLUS:
                    case TOKEN_MINUS:
                        throw new QueryStringParseException("Unexpected character {0}", c);                    
                }

                if (@break)
                {
                    break;
                }

                index++;
            }

            if (index >= size)
            {
                index = size - 1;
            }

            if (index > start)
            {
                string term = AsString(chars, start, index);

                switch (term)
                {
                    case "AND": return new QueryStringToken(QueryStringTokenType.And);
                    case "OR": return new QueryStringToken(QueryStringTokenType.Or);
                    case "NOT": return new QueryStringToken(QueryStringTokenType.Not);
                }

                return new QueryStringTermToken(term);
            }

            throw new QueryStringParseException("Expected term, failed to parse it");
        }

        private QueryStringToken ParseQuery(int start, char[] chars, int size, ref int index)
        {
            int end = index - 1;

            for (int i = start; i <= end; i++)
            {
                char c = chars[i];

                if (c == TOKEN_PERIOD)
                {
                    if (i == end - 2 && chars[i + 1] == TOKEN_BACKSLASH && chars[i + 2] == TOKEN_WILDCARD) //book.\*:
                    {
                        break;
                    }
                    else
                    {
                        throw new QueryStringParseException("Expected .\\*, found something else");
                    }
                }
                else if (!Char.IsLetter(c) && c != TOKEN_UNDERSCORE)
                {
                    throw new QueryStringParseException("Expected letter or _, found {0}", c);
                }
            }

            string field = AsString(chars, start, end);

            index++;
            switch (chars[index])
            {
                case TOKEN_LEFT_CURLY:
                case TOKEN_LEFT_SQUARE:
                    return ParseTwoHandsTermQuery(field, chars, size, ref index);
                case TOKEN_GT:
                case TOKEN_LT:
                    return ParseOneHandTermQuery(field, chars, size, ref index);
                default:
                    return ParseMatchQuery(field, chars, size, ref index);
            }

            throw new QueryStringParseException("Expected QueryStringToken, failed to parse it");
        }

        private QueryStringToken ParseFuzzy(char[] chars, int size, ref int index)
        { 
            index++;
            int start = index;
            int proximity = 0;

            while (index < size)
            {
                char c = chars[index];
                if (c == TOKEN_SPACE || c == TOKEN_RIGHT_PAREN)
                {
                    index--;
                    break;
                }
                else if (!Char.IsDigit(c))
                {
                    throw new QueryStringParseException("Expected numeric value, found {0}", c);
                }

                index++;
            }

            if (index >= size)
            {
                index = size - 1;
            }

            if (start <= index)
            {
                string number = AsString(chars, start, index);
                proximity = int.Parse(number);
            }

            return new QueryStringFuzzyToken(proximity);
        }

        private QueryStringToken ParseBoost(char[] chars, int size, ref int index)
        {
            index++;
            int start = index;
            int boost = 0;

            while (index < size)
            {
                char c = chars[index];
                if (c == TOKEN_SPACE || c == TOKEN_RIGHT_PAREN)
                {
                    index--;
                    break;
                }
                else if (!Char.IsDigit(c))
                {
                    throw new QueryStringParseException("Expected numeric value, found {0}", c);
                }

                index++;
            }

            if (index >= size)
            {
                index = size - 1;
            }

            if (start <= index)
            {
                string number = AsString(chars, start, index);
                boost = int.Parse(number);
                return new QueryStringBoostToken(boost);
            }

            throw new QueryStringParseException("Expected numeric value, found nothing");
        }
        
        private QueryStringQueryToken ParseMatchQuery(string field, char[] chars, int size, ref int index)
        {
            string value = null;

            char c = chars[index];

            if (c == TOKEN_QUOTE)
            {
                value = ParseString(chars, size, ref index);
            }
            else
            {                
                int start = index;
                bool space = false;
                bool regex = false;

                bool group = c == TOKEN_LEFT_PAREN;
                if (group)
                {
                    index++;
                }
                else if (c == TOKEN_FORWARD_SLASH)
                {
                    regex = true;
                }

                while (index < size)
                {
                    c = chars[index];

                    if (c == TOKEN_SPACE && !group)
                    {
                        space = true;
                        break;
                    }
                    else if (c == TOKEN_RIGHT_PAREN)
                    {
                        if (group)
                        {
                            break;
                        }
                        else if (!regex)
                        {
                            space = true;
                            break;
                        }                        
                    }

                    index++;
                }

                if (index >= size)
                {
                    index = size - 1;
                }

                if (index > start)
                {                    
                    value = AsString(chars, start, space ? index - 1 : index);
                }
            }            

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new QueryStringParseException("Expected field value, found nothing");
            }
            else
            {
                return new QueryStringQueryToken(new MatchQuery(field, value));
            }
        }

        private QueryStringQueryToken ParseOneHandTermQuery(string field, char[] chars, int size, ref int index)
        {            
            char c = chars[index];
            bool lt = c == TOKEN_LT;

            bool eq = chars[++index] == TOKEN_EQ;
            if (eq) index++;

            string value = null;

            int start = index;
            bool space = false;

            while (index < size)
            {
                c = chars[index];

                if (c == TOKEN_SPACE)
                {
                    space = true;
                    break;
                }

                index++;
            }

            if (index >= size)
            {
                index = size - 1;
            }

            if (index > start)
            {
                value = AsString(chars, start, space ? index - 1 : index);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new QueryStringParseException("Expected field value, found nothing");
            }
            else
            {
                var rv = new RangeValue(value, eq);
                RangeValues values = lt ? new RangeValues(null, rv) : new RangeValues(rv, null);
                return new QueryStringQueryToken(new RangeQuery(field, values));
            }
        }

        private QueryStringQueryToken ParseTwoHandsTermQuery(string field, char[] chars, int size, ref int index)
        {
            bool eq = chars[index++] == TOKEN_LEFT_SQUARE;

            int start = index;
            RangeValue from = null;
            RangeValue to = null;

            while (index < size)
            {
                if (chars[index] == TOKEN_SPACE && chars[++index] == TOKEN_T && chars[++index] == TOKEN_O && chars[++index] == TOKEN_SPACE)
                {
                    string value = AsString(chars, start, index - 4);
                    from = new RangeValue(value, eq);
                    start = index + 1;                    
                }
                else if (chars[index] == TOKEN_RIGHT_SQUARE)
                {
                    eq = true;
                    break;
                }
                else if (chars[index] == TOKEN_RIGHT_CURLY)
                {
                    eq = false;
                    break;
                }

                index++;
            }

            if (index >= size)
            {
                index = size - 1;
            }

            if (index > start + 1)
            {
                string value = AsString(chars, start, index - 1);
                to = new RangeValue(value, eq);
            }

            if (from == null && to == null)
            {
                throw new QueryStringParseException("Expected range values, found nothing");
            }
            else
            {
                return new QueryStringQueryToken(new RangeQuery(field, new RangeValues(from, to)));
            }
        }

        private string ParseString(char[] chars, int size, ref int index)
        {
            index++;
            int start = index;
            List<int> bs = null; //list of back slashes (aka escapes)

            while (true)
            {
                char c = chars[index++];

                if (c == TOKEN_BACKSLASH)
                {
                    if (bs == null)
                    {
                        bs = new List<int>();
                    }

                    c = chars[index];
                    if (c == TOKEN_R)
                    {
                        chars[index] = TOKEN_CR;
                    }
                    else if (c == TOKEN_N)
                    {
                        chars[index] = TOKEN_LF;
                    }
                    else if (c == TOKEN_T)
                    {
                        chars[index] = TOKEN_HT;
                    }
                    else if (c == TOKEN_B)
                    {
                        chars[index] = TOKEN_BS;
                    }
                    else if (c == TOKEN_F)
                    {
                        chars[index] = TOKEN_FF;
                    }
                    else if (c == TOKEN_A)
                    {
                        chars[index] = TOKEN_BL;
                    }
                    else if (c == TOKEN_V)
                    {
                        chars[index] = TOKEN_VT;
                    }

                    bs.Add(index - 1);

                    index++;
                }
                else if (c == TOKEN_QUOTE)
                {
                    index--;

                    if (bs == null)
                    {
                        return AsString(chars, start - 1, index);
                    }
                    else
                    {
                        int dest = 0;
                        int last = index;
                        int length = index - start - bs.Count;
                        var buffer = new char[length];

                        foreach (int escape in bs)
                        {
                            int len = escape - start;
                            if (len > 0)
                            {
                                Array.Copy(chars, start, buffer, dest, len);
                            }

                            start = escape + 1;
                            dest = dest + len;
                        }

                        if (start < last)
                        {
                            Array.Copy(chars, start, buffer, dest, last - start);
                        }

                        return string.Format("\"{0}\"", AsString(buffer, 0, length - 1));
                    }
                }
            }
        }

        private char SkipWhitespace(char[] chars, int size, ref int index)
        {
            while (index < size)
            {
                char c = chars[index];

                if (c == TOKEN_NULL || c == TOKEN_SPACE || c == TOKEN_LF || c == TOKEN_CR || c == TOKEN_HT || c == TOKEN_BS || c == TOKEN_FF || c == TOKEN_VT)
                {
                    index++;
                }
                else
                {
                    return c;
                }
            }

            return TOKEN_NULL;
        }

        private string AsString(char[] chars, int start, int end)
        {
            int size = end - start + 1;
            var copy = new char[size];
            Array.Copy(chars, start, copy, 0, size);
            return new string(copy);
        }
    }
}