using BlazorPrettyCode;
using BlazorStyled;
using Microsoft.Extensions.DependencyInjection;
using Polished;

namespace SampleCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServicesForSampleSites(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddBlazorStyled(isDevelopment: true);
            serviceCollection.AddSingleton<IMixins, Mixins>();
            serviceCollection.AddSingleton<IShorthand, Shorthand>();
            serviceCollection.AddBlazorPrettyCode();
            return serviceCollection;
        }
    }
}
