using BlazorStyled.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorStyled
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<StyledJsInterop>();
            serviceCollection.AddSingleton<StyleSheet>();
            serviceCollection.AddTransient<IStyled, Styled>();
            return serviceCollection;
        }
    }
}
