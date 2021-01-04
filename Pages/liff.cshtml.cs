using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Wedding.Data;

namespace Wedding.Pages
{
    public class liffModel : PageModel
    {
        public string liffClientId { get; init; }

        public liffModel(IOptionsMonitor<LineBotSetting> setting)
        {
            liffClientId = setting.CurrentValue.LiffClientId;
        }

        public void OnGet()
        {
        }
    }
}
