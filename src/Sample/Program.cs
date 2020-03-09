using BlazorStyled;
using Microsoft.AspNetCore.Blazor.Hosting;
using SampleCore;
using System.Threading.Tasks;

namespace Sample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            //Configure Services

            //AddBlazorStyled is needed for BlazorStyled to work
            builder.Services.AddBlazorStyled(isDevelopment: false, isDebug: true);

            //The following is only used by the sample sites and is not required for BlazorStyled to work
            builder.Services.AddServicesForSampleSites();

            //End Configure Services

            builder.RootComponents.Add<App>("app");

            await builder.Build().RunAsync();
        }
    }
}
