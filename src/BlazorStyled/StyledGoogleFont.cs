using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorStyled
{
    public class StyledGoogleFont : ComponentBase
    {
        [Parameter] public string Name { get; set; }
        [Parameter] public string Styles { get; set; }
        [Inject] private IStyled StyledService { get; set; }

        protected override void OnInitialized()
        {
            List<GoogleFont> googleFonts = new List<GoogleFont>
            {

                new GoogleFont
                {
                    Name = Name,
                    Styles = Styles != null ? new List<string>(Styles.Split(new char[]  {','})) : new List<string> { "400" }
                }
            };
            StyledService.AddGoogleFonts(googleFonts);
        }
    }
}
