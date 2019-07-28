using Microsoft.AspNetCore.Components.Builder;

namespace BlazorStyled
{
    public static class ComponentsApplicationBuilderExtensions
    {
        public static IComponentsApplicationBuilder AddClientSideStyled(this IComponentsApplicationBuilder componentsApplicationBuilder)
        {
            componentsApplicationBuilder.AddComponent<ClientSideStyled>("#styled");
            return componentsApplicationBuilder;
        }
    }
}
