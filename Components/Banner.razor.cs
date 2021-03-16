using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using Wedding.Data;

namespace Wedding.Components
{
    public partial class Banner
    {
        [Parameter]
        public string Style { get; set; } = "";

        [Inject]
        private IConfiguration Configuration { get; init; }
        private static WeddingOptions _weddingOptions;
        private static string _invitationDescription;
        private static string _dateFormat;
        private static string _timeFormat;
        private bool _collapseNavMenu;
        private string NavMenuCssClass => _collapseNavMenu ? "in" : null;
        private void ToggledNavMenu() => _collapseNavMenu = !_collapseNavMenu;

        protected override void OnInitialized()
        {
            _weddingOptions = Configuration.GetSection(nameof(WeddingOptions)).Get<WeddingOptions>();
            _invitationDescription = _weddingOptions.InvitationDescription;
            var cultureInfo = new CultureInfo(_weddingOptions.CultureInfo);
            _dateFormat = _weddingOptions.Date.ToString(_weddingOptions.DayFormat, cultureInfo);
            _timeFormat = _weddingOptions.Date.ToString(_weddingOptions.TimeFormat, cultureInfo);
        }
    }
}
