using System;
using System.Collections.Generic;

namespace YM.Json
{
    class CharJsonParser
    {
        public const char TOKEN_NULL = '\0';
        public const char TOKEN_BL = '\a';
        public const char TOKEN_BS = '\b';
        public const char TOKEN_HT = '\t';
        public const char TOKEN_LF = '\n';
        public const char TOKEN_VT = '\v';
        public const char TOKEN_FF = '\f';
        public const char TOKEN_CR = '\r';
        public const char TOKEN_SPACE = ' ';
        public const char TOKEN_QUOTE = '"';
        public const char TOKEN_COMMA = ',';
        public const char TOKEN_MINUS = '-';
        public const char TOKEN_PERIOD = '.';
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
        public const char TOKEN_COLON = ':';
        public const char TOKEN_LEFT_SQUARE = '[';
        public const char TOKEN_BACKSLASH = '\\';
        public const char TOKEN_RIGHT_SQUARE = ']';
        public const char TOKEN_A = 'a';
        public const char TOKEN_B = 'b';
        public const char TOKEN_E = 'e';
        public const char TOKEN_E_UPPER = 'E';
        public const char TOKEN_F = 'f';
        public const char TOKEN_L = 'l';
        public const char TOKEN_N = 'n';
        public const char TOKEN_R = 'r';
        public const char TOKEN_S = 's';
        public const char TOKEN_T = 't';
        public const char TOKEN_U = 'u';
        public const char TOKEN_V = 'v';
        public const char TOKEN_LEFT_CURLY = '{';
        public const char TOKEN_RIGHT_CURLY = '}';

        public JsonValue Parse(char[] chars)
        {
            if (chars == null || chars.Length == 0)
            {
                return null;
            }

            int index = 0;
            int size = chars.Length;

            char c = SkipWhitespace(chars, size, ref index);
            JsonValue jv = null;

            switch (c)
            {                
                case TOKEN_LEFT_CURLY:
                    jv = ParseObject(chars, size, ref index);
                    break;
                case TOKEN_LEFT_SQUARE:
                    jv = ParseArray(chars, size, ref index);
                    break;
                default:
                    throw new JsonParseException("Expected '{0}' or '{1}', found '{2}'", TOKEN_LEFT_CURLY, TOKEN_LEFT_SQUARE, c);
            }

            index++;
            c = SkipWhitespace(chars, size, ref index);

            if (c == TOKEN_NULL)
            {
                return jv;
            }

            throw new JsonParseException("Expected 'whitespace', found '{0}'", c);
        }

        public JsonValue Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return Parse(json.ToCharArray());
        }


        private JsonObject ParseObject(char[] chars, int size, ref int index)
        {
            var jo = new JsonObject();

            char c = AddProperty(jo, chars, size, ref index);
            while (c == TOKEN_COMMA)
            {
                c = AddProperty(jo, chars, size, ref index);
            }

            if (c != TOKEN_RIGHT_CURLY)
            {
                throw new JsonParseException("Expected '}', found '{0}'", c);
            }

            return jo;
        }

        private string ParsePropertyName(char[] chars, int size, ref int index)
        {
            index++;

            char c = TOKEN_NULL;
            int start = index;

            while (true)
            {
                c = chars[index++];

                if (c == TOKEN_QUOTE)
                {
                    int end = index - 2;

                    if (SkipWhitespace(chars, size, ref index) == TOKEN_COLON)
                    {
                        return AsString(chars, start, end);
                    }
                    else
                    {
                        throw new JsonParseException("Expected '{0}', found '{1}'", TOKEN_COLON, c);
                    }
                }
            }

            throw new JsonParseException("Expected '{0}', found '{1}'", TOKEN_QUOTE, c);
        }

        private JsonValue ParsePropertyValue(char[] chars, int size, ref int index)
        {
            index++;

            char c = SkipWhitespace(chars, size, ref index);

            if (c == TOKEN_QUOTE)
            {
                return new JsonValue(ParseString(chars, size, ref index));
            }

            if (c == TOKEN_LEFT_CURLY)
            {
                return new JsonValue(ParseObject(chars, size, ref index));
            }

            if (c == TOKEN_LEFT_SQUARE)
            {
                return new JsonValue(ParseArray(chars, size, ref index));
            }

            if (c == TOKEN_T)
            {
                ParseTrue(chars, ref index);
                return new JsonValue(true);
            }

            if (c == TOKEN_F)
            {
                ParseFalse(chars, ref index);
                return new JsonValue(false);
            }

            if (c == TOKEN_N)
            {
                ParseNull(chars, ref index);
                return null;
            }

            if (c > TOKEN_COMMA && c < TOKEN_COLON)
            {
                return ParseNumber(chars, ref index);
            }

            throw new JsonParseException("Unexpected character: '{0}'", c);
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
                    int end = index - 1;

                    if (bs == null)
                    {
                        return AsString(chars, start, end);
                    }
                    else
                    {
                        int dest = 0;
                        int last = index;
                        int length = end - start + 1 - bs.Count;
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

                        return AsString(buffer, 0, length - 1);
                    }
                }
            }
        }

        private JsonValue ParseNumber(char[] chars, ref int index)
        {
            int start = index;
            bool floating = false;

            char c = chars[++index];
            while (c > TOKEN_MINUS && c < TOKEN_COLON)
            {
                if (c == TOKEN_PERIOD) floating = true;
                c = chars[++index];
            }

            if (floating && (c == TOKEN_E || c == TOKEN_E_UPPER)) //Scientific
            {
                c = chars[++index]; // skip sign + -
                while (Char.IsDigit(chars[++index])) { } //skip digits
            }

            index--;
            string value = AsString(chars, start, index);

            if (floating)
            {
                if (double.TryParse(value, out double d))
                {
                    return new JsonValue(d);
                }

                if (float.TryParse(value, out float f))
                {
                    return new JsonValue(f);
                }
            }
            else
            {
                if (int.TryParse(value, out int i))
                {
                    return new JsonValue(i);
                }

                if (long.TryParse(value, out long l))
                {
                    return new JsonValue(l);
                }
            }

            throw new JsonParseException("Expected number, found '{0}'", value);
        }

        private JsonArray ParseArray(char[] chars, int size, ref int index)
        {
            var ja = new JsonArray();

            //check for empty array []
            int temp = index;
            index++;
            char c = SkipWhitespace(chars, size, ref index);
            if (c == TOKEN_RIGHT_SQUARE) return ja;
            index = temp;

            while (AddArrayItem(ja, chars, size, ref index) == TOKEN_COMMA) { }

            return ja;
        }

        private void ParseTrue(char[] chars, ref int index)
        {
            if (chars[++index] != TOKEN_R) throw new JsonParseException("Expected 'r', found '{0}'", chars[index]);
            if (chars[++index] != TOKEN_U) throw new JsonParseException("Expected 'u', found '{0}'", chars[index]);
            if (chars[++index] != TOKEN_E) throw new JsonParseException("Expected 'e', found '{0}'", chars[index]);
        }

        private void ParseFalse(char[] chars, ref int index)
        {
            if (chars[++index] != TOKEN_A) throw new JsonParseException("Expected 'a', found '{0}'", chars[index]);
            if (chars[++index] != TOKEN_L) throw new JsonParseException("Expected 'l', found '{0}'", chars[index]);
            if (chars[++index] != TOKEN_S) throw new JsonParseException("Expected 's', found '{0}'", chars[index]);
            if (chars[++index] != TOKEN_E) throw new JsonParseException("Expected 'e', found '{0}'", chars[index]);
        }

        private void ParseNull(char[] chars, ref int index)
        {
            if (chars[++index] != TOKEN_U) throw new JsonParseException("Expected 'u', found '{0}'", chars[index]);
            if (chars[++index] != TOKEN_L) throw new JsonParseException("Expected 'l', found '{0}'", chars[index]);
            if (chars[++index] != TOKEN_L) throw new JsonParseException("Expected 'l', found '{0}'", chars[index]);
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

        private char AddProperty(JsonObject jo, char[] chars, int size, ref int index)
        {
            index++;
            char c = SkipWhitespace(chars, size, ref index);

            if (c == TOKEN_QUOTE)
            {
                string name = ParsePropertyName(chars, size, ref index);
                var value = ParsePropertyValue(chars, size, ref index);
                if (value != null)
                {
                    jo.Add(name, value);
                }

                index++;

                return SkipWhitespace(chars, size, ref index);
            }
            else if (c == TOKEN_RIGHT_CURLY)
            {
                return c;
            }
            else
            {
                throw new JsonParseException("Expected '\"', found '{0}'", c);
            }
        }

        private char AddArrayItem(JsonArray ja, char[] chars, int size, ref int index)
        {
            var value = ParsePropertyValue(chars, size, ref index);
            ja.Add(value);
            index++;
            return SkipWhitespace(chars, size, ref index);
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