using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorStyled.Internal
{
    class StyledJsInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public StyledJsInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task<int> InsertRule(string rule)
        {
            return _jsRuntime.InvokeAsync<int>("styledJsFunctions.insertRule", rule);
        }
    }
}