using System;
using System.Linq;

namespace YM.Elasticsearch
{
    public static class StringExtensions
    {
        public static bool Is(this string s, string compare, bool ignoreSpaces = false)
        {
            string test = ignoreSpaces ? s.Trim() : s;
            return string.Compare(compare, s, true) == 0;
        }

        public static bool Has(this string s, char c)
        {
            foreach (char chr in s)
            {
                if (chr == c) return true;
            }
            return false;
        }

        public static string Escape(this char c)
        {
            return string.Format("\\{0}", c);
        }

        public static string Escape(this string s, char[] escape)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            int length = s.Length;
            var temp = new char[length * 2];
            var chars = s.ToCharArray();
            int src = 0;
            int dest = 0;

            while (src < length)
            {
                char c = chars[src];
                if (escape.Contains(c) && !IsEscaped(chars, src))
                {
                    temp[dest] = '\\';
                    dest++;
                }

                temp[dest] = c;

                src++;
                dest++;
            }

            var buffer = new char[dest];
            Array.Copy(temp, 0, buffer, 0, dest);
            return new string(buffer);
        }

        public static string Unescape(this string s, char[] unescape)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            int length = s.Length;
            var temp = new char[length];
            var chars = s.ToCharArray();
            int src = 0;
            int dest = 0;

            while (src < length)
            {
                char c = chars[src];
                if (unescape.Contains(c) && IsEscaped(chars, src))
                {
                    dest--;
                }

                temp[dest] = c;

                src++;
                dest++;
            }

            var buffer = new char[dest];
            Array.Copy(temp, 0, buffer, 0, dest);
            return new string(buffer);
        }

        public static bool IsEscaped(this char[] chars, int index)
        {
            return index > 0 && chars[index - 1] == '\\';
        }

        public static bool IsPhrase(this string s)
        {
            if (s != null)
            {
                var chars = s.Trim().ToCharArray();
                int last = chars.Length - 1;

                if (last > 1 && chars[0] == '"' && chars[last] == '"' && !chars.IsEscaped(last))
                {
                    int i = 1;
                    while (i < last)
                    {
                        if (chars[i] == '"' && !chars.IsEscaped(i))
                        {
                            return false;
                        }
                        i++;
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool IsPhrasePrefix(this string s)
        {
            if (s != null)
            {
                var chars = s.Trim().ToCharArray();
                int last = chars.Length - 2;

                if (last > 1 && chars[last + 1] == '*' && chars[0] == '"' && chars[last] == '"' && !chars.IsEscaped(last))
                {
                    int i = 1;
                    while (i < last)
                    {
                        if (chars[i] == '"' && !chars.IsEscaped(i))
                        {
                            return false;
                        }
                        i++;
                    }

                    return true;
                }
            }

            return false;
        }

        public static string AsPhrase(this string s, bool ignoreIfNoSpaces = true)
        {
            if (s != null)
            {
                return s.IsPhrase() || (ignoreIfNoSpaces && !s.Has(' ')) ? s : string.Format("\"{0}\"", s.Escape(new char[] { '"' }));
            }

            return null;
        }
    }
}
