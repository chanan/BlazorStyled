using BlazorStyled.Internal;
using BlazorStyled.Stylesheets;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorStyled
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection, bool isDevelopment)
        {
            return AddBlazorStyled(serviceCollection, isDevelopment, false);
        }

        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection, bool isDevelopment, bool isDebug)
        {
            Config config = new Config
            {
                IsDevelopment = isDevelopment,
                IsDebug = isDebug
            };
            if (!serviceCollection.Contains("IConfig"))
            {
                serviceCollection.AddSingleton<IConfig>(config);
            }
            serviceCollection.AddSingleton<IStyleSheet, StyleSheet>();
            serviceCollection.AddTransient<IStyled, StyledImpl>();
            return serviceCollection;
        }

        public static IServiceCollection AddBlazorStyled(this IServiceCollection seriveCollection, Action<IConfig> config)
        {
            IConfig configObj = new Config();
            config(configObj);
            return AddBlazorStyled(seriveCollection, configObj.IsDevelopment, configObj.IsDebug);
        }

        public static IServiceCollection AddBlazorStyled(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddBlazorStyled(false);
        }

        private static bool Contains(this IServiceCollection serviceCollection, string serviceName)
        {
            bool found = false;
            foreach (ServiceDescriptor service in serviceCollection)
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
