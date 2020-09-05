using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly Cache _cache;

        public StyledImpl(ScriptManager scriptManager, Cache cache) : this(scriptManager, cache, DEFAULT, 100_000)
        {

        }

        private StyledImpl(ScriptManager scriptManager, Cache cache, string id, int priority)
        {
            _scriptManager = scriptManager;
            _cache = cache;
            _id = id;
            _priority = priority;
        }

        public async Task<string> CssAsync(string className, string css)
        {
            try
            {
                string preParseHash = css.GetStableHashCodeString();
                if (!_cache.Seen.ContainsKey(preParseHash))
                {
                    css = css.RemoveComments().RemoveDuplicateSpaces();
                    IList<ParsedClass> parsedClasses = css.GetClasses(className);
                    if (parsedClasses.Count > 0)
                    {
                        string hash = parsedClasses.First().IsMediaQuery ? parsedClasses.First().ChildClasses.First().Name.Replace(".", string.Empty) : parsedClasses.First().Name;
                        await _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses);
                        if (!_cache.Seen.ContainsKey(preParseHash))
                        {
                            _cache.Seen.Add(preParseHash, hash);
                        }
                        return hash;
                    }
                    else
                    {
                        if (!_cache.Seen.ContainsKey(preParseHash))
                        {
                            _cache.Seen.Add(preParseHash, string.Empty);
                        }
                        return string.Empty;
                    }
                }
                else
                {
                    return _cache.Seen[preParseHash];
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

        public Task<string> CssAsync(string css)
        {
            return CssAsync((string)null, css);
        }

        public async Task<string> CssAsync(List<string> classes, string css)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string cssClass in classes)
            {
                string result = await CssAsync(cssClass, css);
                sb.Append(result).Append(' ');
            }
            return sb.ToString().Trim();
        }

        public string Css(string className, string css)
        {
            try
            {
                string preParseHash = css.GetStableHashCodeString();
                if (!_cache.Seen.ContainsKey(preParseHash))
                {
                    css = css.RemoveComments().RemoveDuplicateSpaces();
                    IList<ParsedClass> parsedClasses = css.GetClasses(className);
                    if (parsedClasses.Count > 0)
                    {
                        string hash = parsedClasses.First().IsMediaQuery ? parsedClasses.First().ChildClasses.First().Name.Replace(".", string.Empty) : parsedClasses.First().Name;
                        Task.Run(() => _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses));
                        if (!_cache.Seen.ContainsKey(preParseHash))
                        {
                            _cache.Seen.Add(preParseHash, hash);
                        }
                        return hash;
                    }
                    else
                    {
                        if (!_cache.Seen.ContainsKey(preParseHash))
                        {
                            _cache.Seen.Add(preParseHash, string.Empty);
                        }
                        return string.Empty;
                    }
                }
                else
                {
                    return _cache.Seen[preParseHash];
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

        public async Task<string> KeyframesAsync(string css)
        {
            try
            {
                string preParseHash = css.GetStableHashCodeString();
                if (!_cache.Seen.ContainsKey(preParseHash))
                {
                    css = "@keyframes &{" + css.RemoveComments().RemoveDuplicateSpaces() + "}";
                    IList<ParsedClass> parsedClasses = css.GetClasses();
                    await _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses);
                    if (!_cache.Seen.ContainsKey(preParseHash))
                    {
                        _cache.Seen.Add(preParseHash, parsedClasses.First().Hash);
                    }
                    return parsedClasses.First().Hash;
                }
                else
                {
                    return _cache.Seen[preParseHash];
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

        public string Keyframes(string css)
        {
            try
            {
                string preParseHash = css.GetStableHashCodeString();
                if (!_cache.Seen.ContainsKey(preParseHash))
                {
                    css = "@keyframes &{" + css.RemoveComments().RemoveDuplicateSpaces() + "}";
                    IList<ParsedClass> parsedClasses = css.GetClasses();
                    Task.Run(() => _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses));
                    if (!_cache.Seen.ContainsKey(preParseHash))
                    {
                        _cache.Seen.Add(preParseHash, parsedClasses.First().Hash);
                    }
                    return parsedClasses.First().Hash;
                }
                else
                {
                    return _cache.Seen[preParseHash];
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

        public async Task FontfaceAsync(string css)
        {
            try
            {
                await CssAsync(css);
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

        public async Task ClearStylesAsync()
        {
            _cache.Seen.Clear();
            await _scriptManager.ClearStyles(_id.GetStableHashCodeString(), _id);
        }

        public void ClearStyles()
        {
            _cache.Seen.Clear();
            Task.Run(() => _scriptManager.ClearStyles(_id.GetStableHashCodeString(), _id));
        }

        public async Task AddGoogleFontsAsync(List<GoogleFont> googleFonts)
        {
            string fontString = string.Join("|", googleFonts.Select(googleFont => googleFont.Name.Replace(' ', '+') + ':' + string.Join(",", googleFont.Styles)));
            string uri = $"//fonts.googleapis.com/css?family={fontString}&display=swap";
            string preParseHash = uri.GetStableHashCodeString();
            if (!_cache.Seen.ContainsKey(preParseHash))
            {
                IList<ParsedClass> parsedClasses = new List<ParsedClass> { new ParsedClass(uri) };
                await _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses);
            }
        }

        public void AddGoogleFonts(List<GoogleFont> googleFonts)
        {
            string fontString = string.Join("|", googleFonts.Select(googleFont => googleFont.Name.Replace(' ', '+') + ':' + string.Join(",", googleFont.Styles)));
            string uri = $"//fonts.googleapis.com/css?family={fontString}&display=swap";
            string preParseHash = uri.GetStableHashCodeString();
            if (!_cache.Seen.ContainsKey(preParseHash))
            {
                IList<ParsedClass> parsedClasses = new List<ParsedClass> { new ParsedClass(uri) };
                Task.Run(() => _scriptManager.UpdatedParsedClasses(_id.GetStableHashCodeString(), _id, _priority, parsedClasses));
            }
        }

        public IStyled WithId(string id)
        {
            return WithId(id, 1000);  //Default to less than the default sheet (100,000)
        }

        public IStyled WithId(string id, int priority)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = DEFAULT;
                priority = 100_000;
            }
            return new StyledImpl(_scriptManager, _cache, id.Replace(" ", "-"), priority);
        }

        public async Task SetThemeValueAsync(string name, string value)
        {
            await _scriptManager.SetThemeValue(_id.GetStableHashCodeString(), _id, _priority, name, value);
        }

        public void SetThemeValue(string name, string value)
        {
            Task.Run(() => _scriptManager.SetThemeValue(_id.GetStableHashCodeString(), _id, _priority, name, value));
        }

        public async Task<IDictionary<string, string>> GetThemeValuesAsync()
        {
            return await _scriptManager.GetThemeValues(_id.GetStableHashCodeString());
        }

        public IDictionary<string, string> GetThemeValues()
        {
            return Task.Run(() => GetThemeValues()).Result;
        }

        public string GetThemeValue(string name)
        {
            IDictionary<string, string> themeValues = GetThemeValues();
            return themeValues[name];
        }

        public async Task<string> GetThemeValueAsync(string name)
        {
            IDictionary<string, string> themeValues = await GetThemeValuesAsync();
            return themeValues[name];
        }

        public string GetGlobalStyle(string name)
        {
            return _scriptManager.GetGlobalStyle(_id.GetStableHashCodeString(), name);
        }

        public void SetGlobalStyle(string name, string classname)
        {
            _scriptManager.SetGlobalStyle(_id.GetStableHashCodeString(), name, classname);
        }

        public IDictionary<string, string> GetGlobalStyles()
        {
            return _scriptManager.GetGlobalStyles(_id.GetStableHashCodeString());
        }
    }
}
