using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStyled.Internal
{
    internal class StyledImpl : IStyled
    {
        private const string DEFAULT = "Default";
        private readonly ScriptManager _scriptManager;
        private readonly string _id;

        public StyledImpl(ScriptManager scriptManager) : this(scriptManager, DEFAULT)
        {

        }

        private StyledImpl(ScriptManager scriptManager, string id)
        {
            _scriptManager = scriptManager;
            _id = id;
        }

        public string Css(string className, string css)
        {
            try
            {
                css = css.RemoveComments().RemoveDuplicateSpaces();
                IList<ParsedClass> parsedClasses = css.GetClasses(className);
                if (parsedClasses.Count > 0)
                {
                    Task.Run(() => _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, parsedClasses));
                    return parsedClasses.First().Name;
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (StyledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw StyledException.GetException(css, e);
            }
        }

        public string Css(string css)
        {
            return Css((string)null, css);
        }

        public string Css(List<string> classes, string css)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string cssClass in classes)
            {
                string result = Css(cssClass, css);
                sb.Append(result).Append(' ');
            }
            return sb.ToString().Trim();
        }

        public string Keyframes(string css)
        {
            try
            {
                css = "@keyframes &{" + css.RemoveComments().RemoveDuplicateSpaces() + "}";
                IList<ParsedClass> parsedClasses = css.GetClasses();
                Task.Run(() => _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, parsedClasses));
                return parsedClasses.First().Hash;
            }
            catch (StyledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw StyledException.GetException(css, e);
            }
        }

        public void Fontface(string css)
        {
            try
            {
                Css(css);
            }
            catch (StyledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw StyledException.GetException(css, e);
            }
        }

        public void ClearStyles()
        {
            //Not implemented
        }

        public void AddGoogleFonts(List<GoogleFont> googleFonts)
        {
            string fontString = string.Join("|", googleFonts.Select(googleFont => googleFont.Name.Replace(' ', '+') + ':' + string.Join(",", googleFont.Styles)));
            string uri = $"//fonts.googleapis.com/css?family={fontString}&display=swap";
            IList<ParsedClass> parsedClasses = new List<ParsedClass> { new ParsedClass(uri) };
            Task.Run(() => _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, parsedClasses));
        }

        public IStyled WithId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = DEFAULT;
            }
            return new StyledImpl(_scriptManager, id.Replace(" ", "-"));
        }

        public async Task SetThemeValue(string name, string value)
        {
            await _scriptManager.SetThemeValue(_id.GetStableHashCodeString(), _id, name, value);
        }

        public async Task<IDictionary<string, string>> GetThemeValues()
        {
            return await _scriptManager.GetThemeValues(_id.GetStableHashCodeString());
        }

        public async Task SetGlobalStyle(string name, string classname)
        {
            await _scriptManager.SetGlobalStyle(_id.GetStableHashCodeString(), name, classname);
        }

        public async Task<IDictionary<string, string>> GetGlobalStyles()
        {
            return await _scriptManager.GetGlobalStyles(_id.GetStableHashCodeString());
        }

        public async Task<string> GetGlobalStyle(string name)
        {
            return await _scriptManager.GetGlobalStyle(_id.GetStableHashCodeString(), name);
        }
    }
}
