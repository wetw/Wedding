using System.Threading.Tasks;
using LineDC.Liff;
using Microsoft.AspNetCore.Components;
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

        private Customer Customer { get; set; } = new Customer();

        [Inject]
        private ICustomerDao CustomerDao { get; init; }

        [Inject]
        private ILogger<Survey> Logger { get; init; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        private ILiffClient Liff { get; init; }

        private bool IsFilled { get; set; } = true;

        protected override async Task<Task> OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return base.OnAfterRenderAsync(false);
            }

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
                    var customer = (await Liff.GetDecodedIDToken().ConfigureAwait(false)).ToCustomer();
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

                    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                }
            }
            catch (JSDisconnectedException e)
            {
                Logger.LogError(e, "Liff error");
            }

            return base.OnAfterRenderAsync(true);
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
