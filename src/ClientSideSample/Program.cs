using BlazorStyled;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SampleCore;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace ClientSideSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            //Configure Services

            //AddBlazorStyled is needed for BlazorStyled to work
            builder.Services.AddBlazorStyled(isDevelopment: false, isDebug: false);

            //The following is only used by the sample sites and is not required for BlazorStyled to work
            builder.Services.AddServicesForSampleSites();

            //End Configure Services

            builder.RootComponents.Add<App>("app");

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
