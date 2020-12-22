using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Timers;
using Wedding.Data;

namespace Wedding.Components
{
    public partial class Countdown
    {
        [Inject]
        private IConfiguration Configuration { get; init; }
            
        [Parameter]
        public string Id { get; set; } = "countdown";

        private string Days { get; set; } = "000";
        private string Hours { get; set; } = "00";
        private string Minutes { get; set; } = "00";
        private string Seconds { get; set; } = "00";
        private static Timer _timer;
        private static TimeSpan _ts;
        private long _leftTicks;
        private readonly long _secondTicks = TimeSpan.FromSeconds(1).Ticks;
        private static WeddingOptions _weddingOptions;


        protected override void OnInitialized()
        {
            _weddingOptions = Configuration.GetSection(nameof(WeddingOptions)).Get<WeddingOptions>();
            _leftTicks = _weddingOptions.Date.Subtract(DateTime.Now).Ticks;
            _timer = new Timer(1000);
            _timer.Elapsed += CountDownTimer;
            _timer.Enabled = true;
        }

        private async void CountDownTimer(object source, ElapsedEventArgs e)
        {
            if (_leftTicks > 0)
            {
                _leftTicks -= _secondTicks;
                _ts = TimeSpan.FromTicks(_leftTicks);
                Days = _ts.Days.ToString("D3");
                Hours = _ts.Hours.ToString("D2");
                Minutes = _ts.Minutes.ToString("D2");
                Seconds = _ts.Seconds.ToString("D2");
            }
            else
            {
                _timer.Enabled = false;
            }
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        }
    }
}
