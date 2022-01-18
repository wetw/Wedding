using System.Threading;
using System.Threading.Tasks;
using Blazored.Toast.Services;
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

        [Inject]
        private IToastService ToastService { get; init; }

        private bool IsFilled { get; set; } = true;

        private bool IsUpdating { get; set; }

        private string IsShowMask => string.IsNullOrWhiteSpace(Customer?.LineId) ? "mask" : null;

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
                var isInClient = await Liff.IsInClient().ConfigureAwait(false);
                var isLoggedIn = await Liff.IsLoggedIn().ConfigureAwait(false);

                try
                {
                    IsUpdating = true;
                    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                    if (isInClient && isLoggedIn)
                    {
                        await Liff.SendMessages(new { type = "text", text = "我填好了" }).ConfigureAwait(false);
                    }
                }
                catch (JSException e)
                {
                    Logger.LogWarning(e, $"{Customer.Name}'s Liff Error");
                }
                finally
                {
                    IsUpdating = false;
                    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
                }

                if (isInClient)
                {
                    ToastService.ShowSuccess(IsFilled ? "已更新成功，即將關閉視窗" : "填寫成功，即將關閉視窗");
                    await Liff.CloseWindow().ConfigureAwait(false);
                }
                else
                {
                    ToastService.ShowSuccess(IsFilled ? "已更新成功，請關閉視窗" : "填寫成功，請關閉視窗");
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
