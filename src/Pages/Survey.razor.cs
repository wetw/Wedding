using System;
using System.Threading.Tasks;
using LineDC.Liff;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Wedding.Data;
using Wedding.Services;

namespace Wedding.Pages
{
    public partial class Survey
    {
        public EditContext LocalEditContext { get; set; }
        public string ValidationMessage { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private Customer Customer { get; set; }

        [Inject]
        private ICustomerDao CustomerDao { get; init; }

        [Inject]
        private NavigationManager NavigationManager { get; init; }

        [Inject]
        private ILogger<Survey> Logger { get; init; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        private ILiffClient Liff { get; init; }

        private bool IsFilled { get; set; } = true;

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

            var customer = authenticationState?.User?.ToCustomer();
            if (customer is null)
            {
                return;
            }

            // 第一次填寫，或是沒帳號時
            Customer = await CustomerDao.GetByLineIdAsync(customer.LineId).ConfigureAwait(false)
                ?? await CustomerDao.AddAsync(customer).ConfigureAwait(false);
            if (Customer != null && Customer.CreationTime.Equals(Customer.LastModifyTime))
            {
                IsFilled = false;
            }

            if (Customer != null && string.IsNullOrWhiteSpace(Customer.RealName))
            {
                Customer.RealName = Customer.Name;
            }
        }

        protected override async Task<Task> OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (!Liff.Initialized)
                    {
                        await Liff.Init(JSRuntime).ConfigureAwait(false);
                        if (!await Liff.IsLoggedIn().ConfigureAwait(false))
                        {
                            await Liff.Login().ConfigureAwait(false);
                            return Task.CompletedTask;
                        }
                        Liff.Initialized = true;
                    }
                }
                catch (JSDisconnectedException e)
                {
                    Logger.LogError(e, "Liff error");
                }
            }

            return base.OnAfterRenderAsync(firstRender);
        }

        private async Task UpdateAsync()
        {
            if (LocalEditContext.Validate())
            {
                await CustomerDao.UpdateAsync(Customer, Customer.LineId).ConfigureAwait(false);
                Logger.LogInformation($"Updated with: {JsonConvert.SerializeObject(Customer)}");
                try
                {
                    if (await Liff.IsInClient().ConfigureAwait(false))
                    {
                        if (await Liff.IsLoggedIn().ConfigureAwait(false))
                        {
                            await Liff.SendMessages(new { type = "text", text = "我填好了" }).ConfigureAwait(false);
                        }
                        await Liff.CloseWindow().ConfigureAwait(false);
                    }
                }
                catch (JSDisconnectedException e)
                {
                    Logger.LogError(e, "Liff Error");
                }
            }
            else
            {
                ValidationMessage = "資料有錯，請重新修正";
            }
        }

        private void OnEditContestChanged(EditContext context)
        {
            LocalEditContext = context;
        }
    }
}
