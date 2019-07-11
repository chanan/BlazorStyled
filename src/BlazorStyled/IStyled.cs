using BlazorStyled.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public interface IStyled
    {
        Theme Theme { get; set; }
        Task<string> Css(string css);
        Task<string> Css(string className, string css);
        Task<string> Css(List<string> classes, string css);
        Task<string> Keyframes(string css);
        Task Fontface(string css);
    }
}   