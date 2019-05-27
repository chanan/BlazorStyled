using System.Threading.Tasks;

namespace BlazorStyled
{
    public interface IStyled
    {
        Task<string> Css(string css);
        Task<string> Css(string className, string css);
        Task<string> Keyframes(string css);
    }
}