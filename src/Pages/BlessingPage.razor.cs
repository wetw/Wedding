using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Wedding.Data;
using Wedding.Services;

namespace Wedding.Pages
{
    public partial class BlessingPage
    {
        [Inject]
        protected IBlessingDao BlessingDao { get; init; }
        [Inject]
        private NavigationManager NavigationManager { get; init; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }
        private HubConnection _hubConnection;
        private string _messageInput = "";
        private int _blessingTotalCount = 0;
        private IEnumerable<Blessing> _blessingList = Array.Empty<Blessing>();
        private Customer _customer;
        private int _showBlessingCount = 5;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/photoHub"))
                .WithAutomaticReconnect()
                .Build();
            await _hubConnection.StartAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            var authenticationState = await AuthenticationStateTask.ConfigureAwait(false);
            if (authenticationState?.User?.Identity is null
                || !authenticationState.User.Identity.IsAuthenticated)
            {
                var returnUrl = $"/{NavigationManager.ToBaseRelativePath(NavigationManager.Uri)}";
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    NavigationManager.NavigateTo("api/line/login", true);
                }
                NavigationManager.NavigateTo($"/api/line/login?returnUrl={returnUrl}", true);
            }
            _customer = authenticationState?.User.ToCustomer();
            _blessingList = await BlessingDao.GetListAsync(_customer.LineId, pageSize: _showBlessingCount).ConfigureAwait(false);
            _blessingTotalCount= await BlessingDao.CountAsync(_customer.LineId).ConfigureAwait(false);
        }

        public bool IsConnected =>
            _hubConnection?.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task Send()
        {
            try
            {
                if (_hubConnection is not null && !string.IsNullOrWhiteSpace(_messageInput))
                {
                    await _hubConnection.SendAsync("SendMessage", _customer.Name, _messageInput).ConfigureAwait(false);
                }
                var blessing = new Blessing
                {
                    LineId = _customer.LineId,
                    Message = _messageInput
                };
                await BlessingDao.AddAsync(blessing).ConfigureAwait(false);
                _blessingList = _blessingList.Prepend(blessing).Take(_showBlessingCount);
                _blessingTotalCount++;
            }
            finally
            {
                _messageInput = string.Empty;
            }
        }
    }
}
