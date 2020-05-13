using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorStyled.Internal
{
    internal static class StringExtensions
    {
        private static readonly Random rnd = new Random();
        private static readonly Regex comments = new Regex(@"\/\*[\s\S]*?\*\/", RegexOptions.Compiled);
        private static readonly DefaultObjectPoolProvider objectPoolProvider = new DefaultObjectPoolProvider();
        private static readonly ObjectPool<StringBuilder> stringBuilderPool = objectPoolProvider.CreateStringBuilderPool();

        public static string RemoveDuplicateSpaces(this string source)
        {
            int len = source.Length;
            char[] src = source.ToCharArray();
            int dstIdx = 0;
            bool lastWasWS = false;
            for (int i = 0; i < len; i++)
            {
                char ch = src[i];
                if(Char.IsWhiteSpace(ch))
                {
                    if (lastWasWS == false)
                    {
                        src[dstIdx++] = ' ';
                        lastWasWS = true;
                    }
                }
                else
                {
                    lastWasWS = false;
                    src[dstIdx++] = ch;
                }
            }
            return new string(src, 0, dstIdx);
        }

        public static string RemoveComments(this string source)
        {
            return comments.Replace(source, string.Empty);
        }

        public static IList<ParsedClass> GetClasses(this string source)
        {
            return source.GetClasses(null);
        }

        public static IList<ParsedClass> GetClasses(this string source, string classname)
        {
            List<ParsedClass> classes = new List<ParsedClass>();
            ParsedClass root = null, parent = null;
            if (source.IndexOf('{') == -1)
            {
                ParsedClass parsedClass = new ParsedClass(classname, source.Trim());
                classes.Add(parsedClass);
            }
            else
            {
                ReadOnlySpan<char> main = source.AsSpan().Trim();
                bool first = true;
                while (main.IndexOf('{') != -1)
                {
                    int startRules = main.IndexOf('{');
                    int endRules = main.IndexOf('}');
                    ReadOnlySpan<char> classnameSpan = main.Slice(0, startRules).Trim();
                    ReadOnlySpan<char> rules = main.Slice(startRules + 1, endRules - startRules - 1).Trim();
                    if (first && classnameSpan.IndexOf(';') != -1)
                    {
                        int lastSemiColon = classnameSpan.LastIndexOf(';');
                        ParsedClass baseClass = new ParsedClass(classname, main.Slice(0, lastSemiColon + 1).ToString());
                        root = baseClass;
                        classes.Add(baseClass);
                        classnameSpan = classnameSpan.Slice(lastSemiColon + 1);
                    }

                    ParsedClass parsedClass = new ParsedClass(classnameSpan.ToString(), rules.ToString());
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
                        }
                    }
                    if (parsedClass.IsKeyframes || (parsedClass.IsMediaQuery && parsedClass.Declarations == null)) //|| (parsedClass.IsMediaQuery && parsedClass.Declarations != null)
                    {
                        endRules = startRules;
                    }
                    first = false;
                    main = main.Slice(endRules + 1);
                    if (main.TrimStart().IndexOf('}') == 0)
                    {
                        if (main.Length > 2)
                        {
                            main = main.Slice(2);
                        }
                        parent = null;
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
                    if (parsedClass.ChildClasses == null || (parsedClass.ChildClasses != null && parsedClass.ChildClasses.Count > 0))
                    {
                        foreach (ParsedClass child in parsedClass.ChildClasses)
                        {
                            child.Name = child.Name.Replace("&", "." + child.Hash);
                        }
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

        public static uint GetStableHashCode(this string str)
        {
            if (str == null)
            {
                return 0;
            }
            /*unchecked
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
            }*/
            return To32BitFnv1aHash(str);
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
            StringBuilder sb = stringBuilderPool.Get();
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
            string ret = sb.ToString();
            stringBuilderPool.Return(sb);
            return ret;
        }

        private static uint To32BitFnv1aHash(this string toHash, bool separateUpperByte = false)
        {
            IEnumerable<byte> bytesToHash;

            if (separateUpperByte)
                bytesToHash = toHash.ToCharArray()
                    .Select(c => new[] { (byte)((c - (byte)c) >> 8), (byte)c })
                    .SelectMany(c => c);
            else
                bytesToHash = toHash.ToCharArray()
                    .Select(Convert.ToByte);

            //this is the actual hash function; very simple
            uint hash = FnvConstants.FnvOffset32;

            unchecked
            {
                foreach (var chunk in bytesToHash)
                {
                    hash ^= chunk;
                    hash *= FnvConstants.FnvPrime32;
                }
            }
           
            return hash;
        }

        private static class FnvConstants
        {
            public static readonly uint FnvPrime32 = 16777619;
            public static readonly ulong FnvPrime64 = 1099511628211;
            public static readonly uint FnvOffset32 = 2166136261;
            public static readonly ulong FnvOffset64 = 14695981039346656037;
        }
    }
}