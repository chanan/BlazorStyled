using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorStyled
{
    public class StyledGoogleFont : ComponentBase
    {
        [Parameter] private string Name { get; set; }
        [Parameter] private string Styles { get; set; }
        [Inject] private IStyled StyledService { get; set; }

        protected override void OnInit()
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
