using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorStyled.Internal
{
    internal class StyledJsInterop
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IConfig _config;

        public StyledJsInterop(IJSRuntime jsRuntime, IConfig config)
        {
            _jsRuntime = jsRuntime;
            _config = config;
        }

        public Task<int> InsertRule(string rule)
        {
            //return _jsRuntime.InvokeAsync<int>("styledJsFunctions.insertRule", rule, _config.IsDevelopment);
            return Task.FromResult<int>(1);
        }

        public Task ClearAllRules()
        {
            return _jsRuntime.InvokeAsync<bool>("styledJsFunctions.clearAllRules");
        }

        public Task AddGoogleFont(string font)
        {
            return _jsRuntime.InvokeAsync<bool>("styledJsFunctions.addGoogleFont", font);
        }
    }
}