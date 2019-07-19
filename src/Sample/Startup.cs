using BlazorPrettyCode;
using BlazorStyled;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Polished;

namespace Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlazorStyled(isDevelopment: true);
            services.AddSingleton<IMixins, Mixins>();
            services.AddSingleton<IShorthand, Shorthand>();
            services.AddBlazorPrettyCode();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
