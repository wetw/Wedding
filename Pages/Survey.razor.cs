using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Threading.Tasks;
using Wedding.Data;

namespace Wedding.Pages
{
    public partial class Survey
    {
        public EditContext LocalEditContext { get; set; }
        public string ValidationMessage { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }
        private Customer Customer { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authenticationState = await AuthenticationStateTask.ConfigureAwait(false);
            if (authenticationState?.User?.Identity is null
                || !authenticationState.User.Identity.IsAuthenticated)
            {
                var returnUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    NavigationManager.NavigateTo("api/line/login", true);
                }
                NavigationManager.NavigateTo($"/api/line/login?returnUrl={returnUrl}", true);
            }
            Customer = new Customer();
        }

        public void OnGet()
        {
            Console.WriteLine("123");
        }

        private void OnOK()
        {
            ValidationMessage = LocalEditContext.Validate()
                ? "表單驗證正確無誤"
                : "資料有錯，請重新修正";
        }

        private void OnEditContestChanged(EditContext context)
        {
            LocalEditContext = context;
        }
    }
}
