using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
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

            var principal = authenticationState?.User;
            if (principal is null)
            {
                return;
            }

            Customer = await CustomerDao.GetByLineIdAsync(principal.FindFirstValue(ClaimTypes.NameIdentifier)).ConfigureAwait(false);

            // 第一次填寫，或是沒帳號時
            if (Customer is null
                || (Customer != null && Customer.CreationTime.Equals(Customer.LastModifyTime)))
            {
                IsFilled = false;
                Customer = principal.ToCustomer();
            }

            if (string.IsNullOrWhiteSpace(Customer.RealName))
            {
                Customer.RealName = Customer.Name;
            }
        }

        private async Task ConfirmAsync()
        {
            if (LocalEditContext.Validate())
            {
                await CustomerDao.AddAsync(Customer).ConfigureAwait(false);
            }
            else
            {
                ValidationMessage = "資料有錯，請重新修正";
            }
        }

        private async Task UpdateAsync()
        {
            if (LocalEditContext.Validate())
            {
                await CustomerDao.UpdateAsync(Customer, Customer.LineId).ConfigureAwait(false);
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
