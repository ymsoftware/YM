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
        

        public QueryString Parse(char[] chars, bool fixQuery = true)
        {
            if (chars == null || chars.Length == 0)
            {
                return null;
            }

            int index = 0;
            int size = chars.Length;
            int start = 0;
            bool quote = false;
            var tokens = new List<QueryStringToken>();

            SkipWhitespace(chars, size, ref index);

            while (index < size)
            {
                char c = chars[index];

                if (quote)
                {
                    if (c == TOKEN_QUOTE)
                    {
                        quote = false;
                        ParseTerm(start, index, chars, size, tokens);
                        start = index + 1;
                    }
                }
                else
                {
                    switch (c)
                    {
                        case TOKEN_PLUS:
                        case TOKEN_MINUS:
                            ParseSign(c, chars, size, tokens, fixQuery, ref index);
                            start = index;
                            break;
                        case TOKEN_SPACE:
                            ParseTerm(start, index - 1, chars, size, tokens);
                            c = SkipWhitespace(chars, size, ref index);
                            start = index;
                            index--;
                            break;
                        case TOKEN_QUOTE:
                            quote = true;
                            start = index;
                            break;
                        case TOKEN_COLON:
                            ParseQuery(start, chars, size, tokens, fixQuery, ref index);
                            start = index;
                            break;
                        case TOKEN_FUZZY:
                            ParseTerm(start, index - 1, chars, size, tokens);
                            ParseFuzzy(chars, size, tokens, fixQuery, ref index);
                            start = index;
                            break;
                        case TOKEN_BOOST:
                            ParseTerm(start, index - 1, chars, size, tokens);
                            ParseBoost(chars, size, tokens, fixQuery, ref index);
                            start = index;
                            break;
                    }
                }

                index++;
            }

            if (index >= size)
            {
                index = size - 1;
            }

            ParseTerm(start, index, chars, size, tokens);

            return new QueryString(tokens.ToArray());
        }

        public QueryString Parse(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            return Parse(query.ToCharArray());
        }

        private void ParseSign(char sign, char[] chars, int size, List<QueryStringToken> tokens, bool fixQuery, ref int index)
        {
            index++;

            if (index == size)
            {
                if (fixQuery)
                {
                    return;
                }
                else
                {
                    throw new QueryStringParseException("Expected field or term, found EOL");
                }
            }

            char c = chars[index];

            if (Char.IsLetter(c))
            {
                switch (sign)
                {
                    case TOKEN_PLUS:
                        tokens.Add(new QueryStringToken(QueryStringTokenType.Must));
                        return;
                    case TOKEN_MINUS:
                        tokens.Add(new QueryStringToken(QueryStringTokenType.MustNot));
                        return;
                }
            }

            if (fixQuery)
            {
                return;
            }
            else
            {
                throw new QueryStringParseException("Expected field or term, found ' '");
            }
        }

        private void ParseTerm(int start, int end, char[] chars, int size, List<QueryStringToken> tokens)
        {
            if (end > start)
            {
                string term = AsString(chars, start, end);
                tokens.Add(new QueryStringTermToken(term));
            }
        }

        private void ParseQuery(int start, char[] chars, int size, List<QueryStringToken> tokens, bool fixQuery, ref int index)
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
                        if (fixQuery)
                        {
                            index++;
                            return;
                        }
                        else
                        {
                            throw new QueryStringParseException("Expected .\\*, found something else");
                        }
                    }
                }
                else if (!Char.IsLetter(c) && c != TOKEN_UNDERSCORE)
                {
                    if (fixQuery)
                    {
                        index++;
                        return;
                    }
                    else
                    {
                        throw new QueryStringParseException("Expected letter or _, found {0}", c);
                    }
                }
            }

            string field = AsString(chars, start, end);

            QueryStringQueryToken token = null;

            index++;
            switch (chars[index])
            {
                case TOKEN_LEFT_CURLY:
                case TOKEN_LEFT_SQUARE:
                    token = ParseTwoHandsTermQuery(field, chars, size, fixQuery, ref index);
                    break;
                case TOKEN_GT:
                case TOKEN_LT:
                    token = ParseOneHandTermQuery(field, chars, size, fixQuery, ref index);
                    break;
                default:
                    token = ParseMatchQuery(field, chars, size, fixQuery, ref index);
                    break;
            }            

            if (token != null)
            {
                tokens.Add(token);
            }
        }

        private void ParseFuzzy(char[] chars, int size, List<QueryStringToken> tokens, bool fixQuery, ref int index)
        { 
            index++;
            int start = index;
            int end = index;
            int proximity = 0;

            while (index < size)
            {
                char c = chars[index];
                if (c == TOKEN_SPACE)
                {
                    index--;
                    end--;
                    break;
                }
                else if (!Char.IsDigit(c))
                {
                    if (fixQuery)
                    {
                        index--;
                        break;
                    }
                    else
                    {
                        throw new QueryStringParseException("Expected numeric value, found {0}", c);
                    }
                }

                index++;
                end++;
            }

            if (end == size)
            {
                end--;
            }

            if (start <= end)
            {
                string number = AsString(chars, start, end);
                proximity = int.Parse(number);
            }

            tokens.Add(new QueryStringFuzzyToken(proximity));
        }

        private void ParseBoost(char[] chars, int size, List<QueryStringToken> tokens, bool fixQuery, ref int index)
        {
            index++;
            int start = index;
            int end = index;
            int boost = 0;

            while (index < size)
            {
                char c = chars[index];
                if (c == TOKEN_SPACE)
                {
                    index--;
                    end--;
                    break;
                }
                else if (!Char.IsDigit(c))
                {
                    if (fixQuery)
                    {
                        index--;
                        break;
                    }
                    else
                    {
                        throw new QueryStringParseException("Expected numeric value, found {0}", c);
                    }
                }

                index++;
                end++;
            }

            if (end == size)
            {
                end--;
            }

            if (start <= end)
            {
                string number = AsString(chars, start, end);
                boost = int.Parse(number);
            }
            else if (fixQuery)
            {
                throw new QueryStringParseException("Expected numeric value, found nothing");
            }

            tokens.Add(new QueryStringBoostToken(boost));
        }


        private QueryStringQueryToken ParseMatchQuery(string field, char[] chars, int size, bool fixQuery, ref int index)
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

                bool group = c == TOKEN_LEFT_PAREN;
                if (group) index++;

                while (index < size)
                {
                    c = chars[index];

                    if (c == TOKEN_SPACE && !group)
                    {
                        space = true;
                        break;
                    }
                    else if (c == TOKEN_RIGHT_PAREN && group)
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
                    value = AsString(chars, start, space ? index - 1 : index);
                }
            }            

            if (string.IsNullOrWhiteSpace(value))
            {
                if (fixQuery)
                {
                    return null;
                }
                else
                {
                    throw new QueryStringParseException("Expected field value, found nothing");
                }
            }
            else
            {
                return new QueryStringQueryToken(new MatchQuery(field, value));
            }
        }

        private QueryStringQueryToken ParseOneHandTermQuery(string field, char[] chars, int size, bool fixQuery, ref int index)
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
                if (fixQuery)
                {
                    return null;
                }
                else
                {
                    throw new QueryStringParseException("Expected field value, found nothing");
                }
            }
            else
            {
                var rv = new RangeValue(value, eq);
                RangeValues values = lt ? new RangeValues(null, rv) : new RangeValues(rv, null);
                return new QueryStringQueryToken(new RangeQuery(field, values));
            }
        }

        private QueryStringQueryToken ParseTwoHandsTermQuery(string field, char[] chars, int size, bool fixQuery, ref int index)
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
                if (fixQuery)
                {
                    return null;
                }
                else
                {
                    throw new QueryStringParseException("Expected range values, found nothing");
                }
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