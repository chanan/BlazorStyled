using System.Collections.Generic;

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
        void SetThemeValue(string name, string value);
        IEnumerable<KeyValuePair<string, string>> GetThemeValues();
    }
}