using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using BlazorStyled;
using Polished;
using BlazorPrettyCode;

namespace Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlazorStyled(isDevelopment: true);
            services.AddSingleton<IMixins, Mixins>();
            services.AddSingleton<IShorthand, Shorthand>();
            services.AddBlazorPrettyCode(defaultSettings =>
            {
                defaultSettings.IsDevelopmentMode = true;
            });
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
