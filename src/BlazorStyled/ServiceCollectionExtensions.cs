using BlazorStyled.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

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
            IConfig config = new Config
            {
                IsDevelopment = isDevelopment,
                IsDebug = isDebug
            };
            serviceCollection.AddScoped<IConfig>(provider => config);
            serviceCollection.AddScoped<Cache>();
            serviceCollection.AddTransient<IStyled, StyledImpl>();
            serviceCollection.AddScoped<ScriptManager>();
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
    }
}
