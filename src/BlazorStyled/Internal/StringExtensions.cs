using System;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorStyled.Internal
{
    public static class StringExtensions
    {
        private static Random rnd = new Random();

        public static string RemoveDuplicateSpaces(this string source)
        {
            return Regex.Replace(source, @"\s+", " ").Trim();
        }

        public static int GetStableHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static string GetStableHashCodeString(this string str)
        {
            uint i = (uint)str.GetStableHashCode();
            return i.ConvertToBase64Arithmetic();
        }

        public static string GetRandomHashCodeString(this string str)
        {
            uint i = (uint)rnd.Next();
            return i.ConvertToBase64Arithmetic();
        }

        public static string ConvertToBase64Arithmetic(this uint i)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";
            uint length = (uint)alphabet.Length;
            StringBuilder sb = new StringBuilder();
            int pos = 0;
            do
            {
                sb.Append(alphabet[(int)(i % length)]);
                i /= length;
                pos++;
                if (pos == 4)
                {
                    pos = 0;
                    if (i != 0)
                    {
                        sb.Append('-');
                    }
                }
            } while (i != 0);
            return sb.ToString();
        }
    }
}