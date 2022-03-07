using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Wedding.Data;
using Wedding.Data.ReplyIntent;
using Wedding.Services;

namespace Wedding.Pages
{
    public partial class Photo : IAsyncDisposable
    {
        [Inject]
        private IOptionsMonitor<WeddingOptions> Options { get; init; }
        [Inject]
        private NavigationManager NavigationManager { get; init; }
        [Inject]
        protected IBlessingDao BlessingDao { get; init; }
        private HubConnection _hubConnection;
        private string _videoSrc;
        private float _scaling;
        private static Timer _timer;
        private Queue<Blessing> _oldBlessings;
        private readonly Queue<Blessing> _blessings = new();

        protected override void OnInitialized()
        {
            _videoSrc = Options.CurrentValue.WeddingVideoSrc;
            _scaling = Options.CurrentValue.BulletScaling;
            _timer = new Timer(Options.CurrentValue.PhotoBlessingScannerTime);
            _timer.Elapsed += ShowBlessing;
            _timer.Enabled = true;
            _timer.Start();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
            _timer.Dispose();
            _oldBlessings?.Clear();
            _blessings?.Clear();
            GC.SuppressFinalize(this);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(NavigationManager.ToAbsoluteUri("/photoHub"))
                    .WithAutomaticReconnect()
                    .Build();
                _hubConnection.On<string, string>(
                    "ReceiveMessage",
                    (user, message) => _blessings.Enqueue(new Blessing { Name = user, Message = message }));
                await _hubConnection.StartAsync().ConfigureAwait(false);
                await _hubConnection.SendAsync("Subscribe").ConfigureAwait(false);
            }

            await JS.InvokeVoidAsync("SetBulletScreen", _scaling).ConfigureAwait(false);
        }

        private void ShowBlessing(object source, ElapsedEventArgs e)
        {
            _ = InvokeAsync(async () =>
            {
                var count = await JS.InvokeAsync<int>("GetBulletCount").ConfigureAwait(false);
                if (count == 0)
                {
                    if (_oldBlessings is null || !_oldBlessings.Any())
                    {
                        _oldBlessings = new Queue<Blessing>(await BlessingDao.GetListAsync(pageSize: 0, orderBy:x=>x.CreationTime).ConfigureAwait(false));
                    }

                    var blessing = _blessings.Any() ? _blessings.Dequeue() : _oldBlessings.Dequeue();
                    var encodedMsg = $"{blessing.Name}: {blessing.Message}";
                    await JS.InvokeVoidAsync("AddBulletScreen", encodedMsg).ConfigureAwait(false);
                }
            });
        }
    }
}
