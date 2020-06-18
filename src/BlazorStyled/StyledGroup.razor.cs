using System;
using Microsoft.AspNetCore.Components;

namespace BlazorStyled
{
    public partial class StyledGroup : IDisposable
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public bool Loading { get; set; } = true;

        [Parameter]
        public EventCallback<bool> LoadingChanged { get; set; }


        public StyledGroupContext StyleGroupContext { get; set; } = new StyledGroupContext();

        protected override void OnInitialized()
        {
            StyleGroupContext.OnLoadingChanged += OnLoadingChanged;
            base.OnInitialized();
        }

        private async void OnLoadingChanged(bool loading)
        {
            Loading = loading;
            if (LoadingChanged.HasDelegate)
            {
                await InvokeAsync(() => LoadingChanged.InvokeAsync(loading));
            }
        }

        public void Dispose()
        {
            StyleGroupContext.OnLoadingChanged -= OnLoadingChanged;
        }
    }
}
