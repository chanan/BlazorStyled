using BlazorStyled.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorStyled
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection, bool isDevelopment)
        {
            var config = new Config();
            config.IsDevelopment = isDevelopment;
            serviceCollection.AddSingleton<IConfig>(config);
            serviceCollection.AddSingleton<StyledJsInterop>();
            serviceCollection.AddSingleton<StyleSheet>();
            serviceCollection.AddTransient<IStyled, Styled>();
            return serviceCollection;
        }

        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddBlazorStyled(false);
        }
    }
}
