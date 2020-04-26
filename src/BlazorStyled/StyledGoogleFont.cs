using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorStyled
{
    public class StyledGoogleFont : ComponentBase
    {
        [Parameter] public string Id { get; set; }
        [Parameter] public int? Priority { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string Styles { get; set; }
        [Inject] private IStyled StyledService { get; set; }

        protected override void OnInitialized()
        {
            IStyled styled = Id == null ? StyledService : Priority.HasValue ? StyledService.WithId(Id, Priority.Value) : StyledService.WithId(Id);
            List<GoogleFont> googleFonts = new List<GoogleFont>
            {

                new GoogleFont
                {
                    Name = Name,
                    Styles = Styles != null ? new List<string>(Styles.Split(new char[]  {','})) : new List<string> { "400" }
                }
            };
            styled.AddGoogleFonts(googleFonts);
        }
    }
}
