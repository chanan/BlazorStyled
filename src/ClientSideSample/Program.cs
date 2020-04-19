using BlazorStyled;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SampleCore;
using System.Threading.Tasks;

namespace ClientSideSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            //Configure Services

            //AddBlazorStyled is needed for BlazorStyled to work
            builder.Services.AddBaseAddressHttpClient();
            builder.Services.AddBlazorStyled(isDevelopment: false, isDebug: false);

            //The following is only used by the sample sites and is not required for BlazorStyled to work
            builder.Services.AddServicesForSampleSites();

            //End Configure Services

            builder.RootComponents.Add<App>("app");

            builder.Services.AddBaseAddressHttpClient();

            await builder.Build().RunAsync();
        }
    }
}
