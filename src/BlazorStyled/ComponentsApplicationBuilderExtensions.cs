using Microsoft.AspNetCore.Components.Builder;

namespace BlazorStyled
{
    public static class ComponentsApplicationBuilderExtensions
    {
        public static IComponentsApplicationBuilder AddClientSideStyled(this IComponentsApplicationBuilder componentsApplicationBuilder, string id)
        {
            componentsApplicationBuilder.AddComponent<ClientSideStyled>("#" + id);
            return componentsApplicationBuilder;
        }
    }
}
