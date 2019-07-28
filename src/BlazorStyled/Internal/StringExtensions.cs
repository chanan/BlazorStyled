using System.Text;

namespace BlazorStyled.Internal
{
    public static class StringExtensions
    {
        public static string RemoveUnneededSpaces(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            bool bFound = false;
            StringBuilder sb = new StringBuilder(source.Length);
            foreach (char chr in source.Trim())
            {
                if (chr == ' ')
                {
                    if (bFound)
                    {
                        continue;
                    }
                    bFound = true;
                }
                else
                {
                    bFound = false;
                }
                sb.Append(chr);
            }
            return sb.ToString();
        }
    }
}