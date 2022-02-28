using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.Message;
using Wedding.Data;
using Wedding.Services;

namespace Wedding.Pages
{
    partial class LuckyDrawing
    {
        [Inject]
        protected IBlessingDao BlessingDao { get; init; }
        [Inject]
        private ICustomerDao CustomerDao { get; init; }
        [Inject]
        private ILineMessageUtility LineMessageUtility { get; init; }
        [Inject]
        private IOptionsMonitor<WeddingOptions> Options { get; init; }
        [Inject]
        private NavigationManager NavigationManager { get; init; }
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }
        private string _buttonStartClass;
        private string _animationStartClass;
        private readonly ConcurrentDictionary<string, Customer> _luckyMans = new();

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
        }

        private async Task StartDrawing()
        {
            _buttonStartClass = "disappear";
            _animationStartClass = "animation";
            if (_luckyMans.Any())
            {
                return;
            }

            await GetLuckyMans().ConfigureAwait(false);
        }

        private async Task GetLuckyMans()
        {
            var list = await BlessingDao.GetListAsync(pageSize: 0).ConfigureAwait(false);
            var rnd = new Random(int.TryParse(Options.CurrentValue.RandomKey, out var key) ? key : new Random().Next());
            var maxLuckyManCount = list.GroupBy(x => x.LineId).Count();
            while (_luckyMans.Count < Math.Min(Options.CurrentValue.LuckyManCount, maxLuckyManCount))
            {
                var blessing = list.OrderBy(x => rnd.Next()).First();
                _luckyMans.TryAdd(blessing.LineId, await CustomerDao.GetByLineIdAsync(blessing.LineId).ConfigureAwait(false));
            }
            if (Options.CurrentValue.PushMessageToLuckyMan)
            {
                foreach (var item in _luckyMans)
                {
                    await LineMessageUtility.PushMessageAsync(
                        item.Key,
                        new List<IRequestMessage> { new TextMessage("恭喜你中獎啦~~\r\n趕快上來領取獎品喔") }).ConfigureAwait(false);
                }
            }
        }
    }
}
