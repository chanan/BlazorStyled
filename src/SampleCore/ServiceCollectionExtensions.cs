using BlazorPrettyCode;
using BlazorTypography;
using Microsoft.Extensions.DependencyInjection;
using Polished;

namespace SampleCore
{
    public static class ServiceCollectionExtensions
    {
        //Theese are common services that the sample site use and are not requied by BlazorStlyed
        public static IServiceCollection AddServicesForSampleSites(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IMixins, Mixins>();
            serviceCollection.AddSingleton<IShorthand, Shorthand>();
            serviceCollection.AddBlazorPrettyCode();
            serviceCollection.AddTypography();
            return serviceCollection;
        }
    }
}