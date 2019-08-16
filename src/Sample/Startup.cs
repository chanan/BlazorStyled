using BlazorStyled;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using SampleCore;

namespace Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //AddBlazorStyled is needed for BlazorStyled to work
            services.AddBlazorStyled();

            //The following is only used by the sample sites and is not required for BlazorStyled to work
            services.AddServicesForSampleSites();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
            app.AddClientSideStyled();
        }
    }
}
