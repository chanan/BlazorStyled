using System.Threading.Tasks;

namespace BlazorStyled
{
    public interface IStyled
    {
        Task<string> Css(string css);
    }
}