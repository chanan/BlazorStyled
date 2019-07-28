using System.Text.RegularExpressions;

namespace BlazorStyled.Internal
{
    public static class StringExtensions
    {
        public static string RemoveDuplicateSpaces(this string source)
        {
            return Regex.Replace(source, @"\s+", " ").Trim();
        }
    }
}