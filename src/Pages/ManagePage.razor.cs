using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Wedding.Data;
using Wedding.Services;

namespace Wedding.Pages
{
    partial class ManagePage
    {

        [Inject]
        private ICustomerDao CustomerDao { get; init; }

        [Inject]
        private NavigationManager NavigationManager { get; init; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; init; }

        private IList<Customer> _customers;

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
            _customers = await CustomerDao.GetListAsync(1, 10).ConfigureAwait(false);
        }
    }

}
