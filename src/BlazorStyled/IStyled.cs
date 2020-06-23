using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public interface IStyled
    {
        Task ClearStylesAsync();
        void ClearStyles();
        Task<string> CssAsync(string css);
        Task<string> CssAsync(string className, string css);
        Task<string> CssAsync(List<string> classes, string css);
        string Css(string css);
        string Css(string className, string css);
        string Css(List<string> classes, string css);
        Task<string> KeyframesAsync(string css);
        Task FontfaceAsync(string css);
        Task AddGoogleFontsAsync(List<GoogleFont> googleFonts);
        string Keyframes(string css);
        void Fontface(string css);
        void AddGoogleFonts(List<GoogleFont> googleFonts);
        IStyled WithId(string id);
        IStyled WithId(string id, int priority = 1000);
        Task SetThemeValueAsync(string name, string value);
        void SetThemeValue(string name, string value);
        Task<IDictionary<string, string>> GetThemeValuesAsync();
        IDictionary<string, string> GetThemeValues();
        string GetThemeValue(string name);
        Task<string> GetThemeValueAsync(string name);
        void SetGlobalStyle(string name, string classname);
        IDictionary<string, string> GetGlobalStyles();
        string GetGlobalStyle(string name);
    }
}