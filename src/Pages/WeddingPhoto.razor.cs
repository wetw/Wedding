using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Wedding.Data;

namespace Wedding.Pages
{
    public partial class WeddingPhoto : IAsyncDisposable
    {
        [Inject]
        private IOptionsMonitor<WeddingOptions> Options { get; init; }

        [Inject]
        private NavigationManager NavigationManager { get; init; }

        private HubConnection _hubConnection;
        private string _videoSrc;
        private float _scaling;

        protected override void OnInitialized()
        {
            _videoSrc = Options.CurrentValue.WeddingVideoSrc;
            _scaling = Options.CurrentValue.BulletScaling;
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(NavigationManager.ToAbsoluteUri("/photoHub"))
                    .WithAutomaticReconnect()
                    .Build();
                _hubConnection.On<string, string>("ReceiveMessage", async (user, message) =>
                {
                    var encodedMsg = $"{user}: {message}";
                    await JS.InvokeVoidAsync("AddBulletScreen", encodedMsg).ConfigureAwait(false);
                });
                await _hubConnection.StartAsync().ConfigureAwait(false);
                await _hubConnection.SendAsync("Subscribe").ConfigureAwait(false);
            }

            await JS.InvokeVoidAsync("SetBulletScreen", _scaling).ConfigureAwait(false);
        }
    }
}
