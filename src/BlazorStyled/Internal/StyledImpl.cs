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
        private readonly int _priority;

        public StyledImpl(ScriptManager scriptManager) : this(scriptManager, DEFAULT, int.MaxValue)
        {

        }

        private StyledImpl(ScriptManager scriptManager, string id, int priority)
        {
            _scriptManager = scriptManager;
            _id = id;
            _priority = priority;
        }

        public async Task<string> Css(string className, string css)
        {
            try
            {
                css = css.RemoveComments().RemoveDuplicateSpaces();
                IList<ParsedClass> parsedClasses = css.GetClasses(className);
                if (parsedClasses.Count > 0)
                {
                    await _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses);
                    return parsedClasses.First().IsMediaQuery ? parsedClasses.First().ChildClasses.First().Name.Replace(".", string.Empty) : parsedClasses.First().Name;
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

        public Task<string> Css(string css)
        {
            return Css((string)null, css);
        }

        public async Task<string> Css(List<string> classes, string css)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string cssClass in classes)
            {
                string result = await Css(cssClass, css);
                sb.Append(result).Append(' ');
            }
            return sb.ToString().Trim();
        }

        public async Task<string> Keyframes(string css)
        {
            try
            {
                css = "@keyframes &{" + css.RemoveComments().RemoveDuplicateSpaces() + "}";
                IList<ParsedClass> parsedClasses = css.GetClasses();
                await _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses);
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

        public async Task Fontface(string css)
        {
            try
            {
                await Css(css);
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

        public async Task ClearStyles()
        {
            await _scriptManager.ClearStyles(_id.GetStableHashCodeString(), _id);
        }

        public async Task AddGoogleFonts(List<GoogleFont> googleFonts)
        {
            string fontString = string.Join("|", googleFonts.Select(googleFont => googleFont.Name.Replace(' ', '+') + ':' + string.Join(",", googleFont.Styles)));
            string uri = $"//fonts.googleapis.com/css?family={fontString}&display=swap";
            IList<ParsedClass> parsedClasses = new List<ParsedClass> { new ParsedClass(uri) };
            await _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses);
        }

        public IStyled WithId(string id)
        {
            return WithId(id, 1000);
        }

        public IStyled WithId(string id, int priority)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = DEFAULT;
                priority = int.MaxValue;
            }
            return new StyledImpl(_scriptManager, id.Replace(" ", "-"), priority);
        }

        public async Task SetThemeValue(string name, string value)
        {
            await _scriptManager.SetThemeValue(_id.GetStableHashCodeString(), _id, _priority, name, value);
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
