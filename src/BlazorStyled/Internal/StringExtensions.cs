using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorStyled.Internal
{
    internal static class StringExtensions
    {
        private static readonly Random rnd = new Random();

        public static string RemoveDuplicateSpaces(this string source)
        {
            return Regex.Replace(source, @"\s+", " ").Trim();
        }

        public static string RemoveComments(this string source)
        {
            return Regex.Replace(source, @"\/\*[\s\S]*?\*\/", string.Empty).Trim();
        }

        public static IList<ParsedClass> GetClasses(this string source)
        {
            return source.GetClasses(null);
        }

        public static IList<ParsedClass> GetClasses(this string source, string classname)
        {
            List<ParsedClass> classes = new List<ParsedClass>();
            string[] rules = Regex.Split(source, @"[\@\#\.\w\-\,\s\n\r\t&%:()]+(?=\s*\{)");
            MatchCollection classnames = Regex.Matches(source, @"[\@\#\.\w\-\,\s\n\r\t&%:()]+(?=\s*\{)");
            ParsedClass root = null, parent = null;
            if (rules[0].Trim() != string.Empty)
            {
                ParsedClass parsedClass = new ParsedClass(classname, rules[0].Trim());
                root = parsedClass;
                classes.Add(parsedClass);
            }
            else if (rules[0].Trim() == string.Empty && classname != null)
            {
                ParsedClass parsedClass = new ParsedClass(classname, string.Empty);
                root = parsedClass;
                classes.Add(parsedClass);
            }
            for (int i = 0; i < classnames.Count; i++)
            {
                ParsedClass parsedClass = new ParsedClass(classnames[i].Value.Trim(), rules[i + 1].Trim());
                if (parsedClass.IsParent)
                {
                    parent = parsedClass;
                    if (root == null)
                    {
                        classes.Add(parsedClass);
                    }
                    else
                    {
                        root.ChildClasses.Add(parsedClass);
                    }
                }
                else
                {
                    if (parent == null)
                    {
                        if (root == null)
                        {
                            classes.Add(parsedClass);
                        }
                        else
                        {
                            root.ChildClasses.Add(parsedClass);
                        }
                    }
                    else
                    {
                        parent.ChildClasses.Add(parsedClass);
                        if (parsedClass.IsLastChild)
                        {
                            parent = null;
                        }
                    }
                }
            }
            List<ParsedClass> ret = new List<ParsedClass>();
            foreach (ParsedClass parsedClass in classes)
            {
                if (parsedClass.IsDynamic)
                {
                    ret.Add(parsedClass);
                    foreach (ParsedClass child in parsedClass.ChildClasses)
                    {
                        child.Parent = parsedClass.Name;
                        child.Name = child.Name.Replace("&", "." + parsedClass.Name);
                        ret.Add(child);
                    }
                }
                else if (parsedClass.IsMediaQuery)
                {
                    if (parsedClass.ChildClasses == null || parsedClass.ChildClasses != null && parsedClass.ChildClasses.Count > 0)
                    {
                        ret.Add(parsedClass);
                    }
                }
                else
                {
                    ret.Add(parsedClass);
                }
            }
            return ret;
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