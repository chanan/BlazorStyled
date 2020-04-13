using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public interface IStyled
    {
        Task ClearStyles();
        Task<string> Css(string css);
        Task<string> Css(string className, string css);
        Task<string> Css(List<string> classes, string css);
        Task<string> Keyframes(string css);
        Task Fontface(string css);
        Task AddGoogleFonts(List<GoogleFont> googleFonts);
        IStyled WithId(string id);
        IStyled WithId(string id, int priority = 1000);
        Task SetThemeValue(string name, string value);
        Task<IDictionary<string, string>> GetThemeValues();
        Task SetGlobalStyle(string name, string classname);
        Task<IDictionary<string, string>> GetGlobalStyles();
        Task<string> GetGlobalStyle(string name);
    }
}