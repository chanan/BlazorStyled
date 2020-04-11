using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public interface IStyled
    {
        void ClearStyles();
        string Css(string css);
        string Css(string className, string css);
        string Css(List<string> classes, string css);
        string Keyframes(string css);
        void Fontface(string css);
        void AddGoogleFonts(List<GoogleFont> googleFonts);
        IStyled WithId(string id);
        Task SetThemeValue(string name, string value);
        Task<IDictionary<string, string>> GetThemeValues();
        Task SetGlobalStyle(string name, string classname);
        Task<IDictionary<string, string>> GetGlobalStyles();
        Task<string> GetGlobalStyle(string name);
    }
}