using Microsoft.AspNetCore.Components.Builder;

namespace BlazorStyled
{
    public static class ComponentsApplicationBuilderExtensions
    {
        public static IComponentsApplicationBuilder AddStyled(this IComponentsApplicationBuilder componentsApplicationBuilder, string id)
        {
            componentsApplicationBuilder.AddComponent<Styled>("#" + id);
            return componentsApplicationBuilder;
        }
    }
}
