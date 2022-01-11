using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private ILogger<Survey> Logger { get; set; }

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

        private async Task UpdateAsync()
        {
            if (LocalEditContext.Validate())
            {
                await CustomerDao.UpdateAsync(Customer, Customer.LineId).ConfigureAwait(false);
                Logger.LogInformation($"Updated with: {JsonConvert.SerializeObject(Customer)}");
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
