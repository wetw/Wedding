using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Wedding.Data;

namespace Wedding.Pages
{
    public partial class WeddingPhoto
    {
        [Inject]
        private IOptionsMonitor<WeddingOptions> Options { get; init; }

        [Inject]
        private NavigationManager NavigationManager { get; init; }

        private HubConnection _hubConnection;
        private readonly IList<string> _messages = new List<string>();

        private IReadOnlyCollection<object> _dataSource;

        protected override async Task OnInitializedAsync()
        {
            var random = new Random();

            _dataSource = Options.CurrentValue.WeddingPhotos
                .Select(x => (object)new { image = x })
                .OrderBy(e => random.Next())
                .ToList();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/photoHub"))
                .WithAutomaticReconnect()
                .Build();
            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodedMsg = $"{user}: {message}";
                _messages.Add(encodedMsg);
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}