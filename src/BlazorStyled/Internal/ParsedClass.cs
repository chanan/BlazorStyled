using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class ParsedClass
    {
        public ParsedClass(string name, string body)
        {
            if (name == null)
            {
                IsDynamic = true;
                ChildClasses = new List<ParsedClass>();
            }
            else
            {
                IsDynamic = false;
                Name = name;
            }
            if (IsMediaQuery)
            {
                ChildClasses = new List<ParsedClass>();
            }
            if (body.EndsWith("} }") || body.EndsWith("}}"))
            {
                Declarations = ParseDeclerations(body.Substring(0, body.Length - 1));
                IsLastChild = true;
            }
            else if (body == "{")
            {
                ChildClasses = new List<ParsedClass>();
            }
            else
            {
                Declarations = ParseDeclerations(body);
                if (IsDynamic)
                {
                    Name = Hash;
                }
            }
        }

        public ParsedClass(string importUri)
        {
            ImportUri = importUri;
            IsDynamic = false;
            Name = Hash;
        }

        private IDictionary<string, string> ParseDeclerations(string body)
        {
            IDictionary<string, string> declarations = new SortedDictionary<string, string>();
            string str = body.Trim();
            str = str.StartsWith("{") && str.EndsWith("}") ? str.Substring(1, str.Trim().Length - 2).Trim() : str;
            string[] declarationsString = str.Split(';');
            foreach (string declarationString in declarationsString)
            {
                if (declarationString.IndexOf(':') != -1)
                {
                    Tuple<string, string> declaration = ParseDeclaration(declarationString.Trim());
                    if (declaration != null && declaration.Item1 != "label")
                    {
                        if (declarations.ContainsKey(declaration.Item1))
                        {
                            declarations[declaration.Item1] = declaration.Item2;
                        }
                        else
                        {
                            declarations.Add(declaration.Item1, declaration.Item2);
                        }
                    }
                    else
                    {
                        Label = declaration.Item2;
                    }
                }
            }
            return declarations;
        }

        private Tuple<string, string> ParseDeclaration(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            try
            {
                string property = input.Substring(0, input.IndexOf(':')).ToLower().Trim();
                string value = input.Substring(input.IndexOf(':') + 1).Trim();
                return new Tuple<string, string>(property, value);
            }
            catch (Exception e)
            {
                throw StyledException.GetException(input, "This is likely cause by a missing ':' character", e);
            }
        }

        public string Name { get; set; }
        public string Label { get; private set; }
        public IList<ParsedClass> ChildClasses { get; private set; }
        public bool IsDynamic { get; private set; }
        public bool IsParent => ChildClasses != null;
        public bool IsMediaQuery => !IsDynamic && Name.IndexOf("@media") != -1;
        public bool IsFontface => !IsDynamic && Name.IndexOf("@font-face") != -1;
        public bool IsKeyframes => !IsDynamic && IsParent && Name.IndexOf("@keyframe") != -1;
        public bool IsElement => !IsDynamic && !IsFontface && !IsKeyframes && !IsMediaQuery; //TODO: This might not be correct
        public bool IsLastChild { get; private set; }
        public string Parent { get; set; }
        public IDictionary<string, string> Declarations { get; private set; } = new SortedDictionary<string, string>();
        public string ImportUri { get; private set; }
        public bool IsImportUri => ImportUri != null;

        public bool MergeDecelerations(IDictionary<string, string> declarations)
        {
            bool changed = false;
            foreach (string property in declarations.Keys)
            {
                if (property != "label")
                {
                    if (Declarations.ContainsKey(property))
                    {
                        if (Declarations[property] != declarations[property])
                        {
                            Declarations[property] = declarations[property];
                            changed = true;
                        }
                    }
                    else
                    {
                        Declarations.Add(property, declarations[property]);
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                _hash = null;
                if (IsDynamic)
                {
                    Name = Hash;
                }
            }
            return changed;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (IsImportUri)
            {
                sb.Append("@import url('").Append(ImportUri).Append("');");
            }
            if (Declarations.Count > 0)
            {
                if (IsDynamic)
                {
                    sb.Append('.');
                }
                sb.Append(Name).Append('{');
                if (Parent != null && IsMediaQuery)
                {
                    sb.Append('.').Append(Parent).Append('{');
                }
                foreach (string property in Declarations.Keys)
                {
                    sb.Append(property).Append(':').Append(Declarations[property]).Append(';');
                }
                if (Parent != null && IsMediaQuery)
                {
                    sb.Append('}');
                }
                sb.Append('}');
            }
            if (!IsDynamic && IsParent && ChildClasses.Count > 0)
            {
                string name = IsKeyframes ? Name.Replace("&", Hash) : Name;
                sb.Append(name).Append('{');
                foreach (ParsedClass parsedClass in ChildClasses)
                {
                    sb.Append(parsedClass.ToString());
                }
                sb.Append('}');
            }
            return sb.ToString();
        }

        private string ToStringForHash()
        {
            StringBuilder sb = new StringBuilder();
            if (IsImportUri)
            {
                sb.Append("@import url('").Append(ImportUri).Append("');");
            }
            if (Declarations.Count > 0)
            {
                if (IsDynamic)
                {
                    sb.Append('.');
                }
                sb.Append(Name).Append('{');
                if (Parent != null && IsMediaQuery)
                {
                    sb.Append('.').Append(Parent).Append('{');
                }
                foreach (string property in Declarations.Keys)
                {
                    sb.Append(property).Append(':').Append(Declarations[property]).Append(';');
                }
                if (Parent != null && IsMediaQuery)
                {
                    sb.Append('}');
                }
                sb.Append('}');
            }
            if (!IsDynamic && IsParent && ChildClasses.Count > 0)
            {
                sb.Append(Name).Append('{');
                foreach (ParsedClass parsedClass in ChildClasses)
                {
                    sb.Append(parsedClass.ToString());
                }
                sb.Append('}');
            }
            return sb.ToString();
        }

        private string _hash = null;
        public string Hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = Label != null ? ToStringForHash().GetStableHashCodeString() + "-" + Label : ToStringForHash().GetStableHashCodeString();
                }
                return _hash;
            }
        }
    }
}
