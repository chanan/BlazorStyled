using BlazorStyled.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorStyled
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection, bool isDevelopment)
        {
            var config = new Config
            {
                IsDevelopment = isDevelopment
            };
            if(!serviceCollection.Contains("IConfig"))
            {
                serviceCollection.AddSingleton<IConfig>(config);
            }
            serviceCollection.AddTransient<StyledJsInterop>();
            serviceCollection.AddSingleton<StyleSheet>();
            serviceCollection.AddTransient<IStyled, Styled>();
            return serviceCollection;
        }

        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddBlazorStyled(false);
        }

        private static bool Contains(this IServiceCollection serviceCollection, string serviceName)
        {
            bool found = false;
            foreach (var service in serviceCollection)
            {
                if (service.ServiceType != null && service.ServiceType.Name == serviceName)
                {
                    found = true;
                }
            }
            return found;
        }
    }
}
