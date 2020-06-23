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
            if ((IsMediaQuery && name.Contains("&")) || (IsMediaQuery & body.Contains("{")) || IsKeyframes)
            {
                ChildClasses = new List<ParsedClass>();
                body = null;
            }
            else if (IsMediaQuery)
            {
                ChildClasses = new List<ParsedClass>();
            }
            if (body != null && (body.EndsWith("} }") || body.EndsWith("}}")))
            {
                Declarations = ParseDeclerations(body.Substring(0, body.Length - 1));
                IsLastChild = true;
            }
            else if (body != null && body == "{") //TODO: This might not be needed anymore
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

        private string ParseDeclerations(string body)
        {
            if (body == null)
            {
                return null;
            }

            string str = body.Trim();
            if (str.Contains("label"))
            {
                int start = str.IndexOf(":", str.IndexOf("label"));
                if (start != -1)
                {
                    int end = str.IndexOf(";", start);
                    if (end != -1)
                    {
                        Label = str.Substring(start + 1, end - start - 1).Trim();
                        str = str.Substring(0, start - 5) + str.Substring(end + 1, str.Length - end - 1).Trim();
                    }
                }
            }
            return str.StartsWith("{") && str.EndsWith("}") ? str.Substring(1, str.Trim().Length - 2).Trim() : str;
        }

        public string Name { get; set; }
        public string Label { get; private set; }
        public IList<ParsedClass> ChildClasses { get; private set; }
        public bool IsDynamic { get; private set; }
        public bool IsParent => ChildClasses != null;
        public bool IsMediaQuery => !IsDynamic && Name.IndexOf("@media") != -1;
        public bool IsFontface => !IsDynamic && Name.IndexOf("@font-face") != -1;
        public bool IsKeyframes => !IsDynamic && Name.IndexOf("@keyframe") != -1;
        public bool IsElement => !IsDynamic && !IsFontface && !IsKeyframes && !IsMediaQuery; //TODO: This might not be correct
        public bool IsLastChild { get; private set; }
        public string Parent { get; set; }
        public string Declarations { get; private set; }
        public string ImportUri { get; private set; }
        public bool IsImportUri => ImportUri != null;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (IsImportUri)
            {
                sb.Append("@import url('").Append(ImportUri).Append("');");
            }
            if (Declarations != null)
            {
                if (IsDynamic)
                {
                    sb.Append('.');
                }
                sb.Append(Name).Append('{');
                if (IsMediaQuery && Parent != null)
                {
                    sb.Append('.').Append(Parent).Append('{');
                }
                sb.Append(Declarations);
                if (IsMediaQuery && Parent != null)
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
            if (Declarations != null)
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
                sb.Append(Declarations);
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
