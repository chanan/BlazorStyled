using System;
using Microsoft.AspNetCore.Components;

namespace BlazorStyled
{
    public partial class StyledGroup : IDisposable
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool Loading { get; set; } = true;
        [Parameter] public EventCallback<bool> LoadingChanged { get; set; }
        [Parameter] public TimeSpan DebouncerTimeout { get; set; } = TimeSpan.Zero;

        public StyledGroupContext StyleGroupContext { get; set; } = new StyledGroupContext();

        protected override void OnInitialized()
        {
            StyleGroupContext.OnLoadingChanged += OnLoadingChanged;
            StyleGroupContext.SetRendering(true);
        }

        protected override void OnParametersSet()
        {
            StyleGroupContext.DebouncerTimeout = DebouncerTimeout;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            StyleGroupContext.SetRendering(false);
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
