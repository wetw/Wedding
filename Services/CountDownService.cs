using Microsoft.Extensions.Options;
using System;
using System.Timers;
using Wedding.Data;

namespace Wedding.Services
{
    public class CountDownService
    {
        private string Days { get; set; } = "000";
        private string Hours { get; set; } = "00";
        private string Minutes { get; set; } = "00";
        private string Seconds { get; set; } = "00";
        private static Timer _timer;
        private static TimeSpan _ts;
        private long _leftTicks;
        private readonly long _secondTicks = TimeSpan.FromSeconds(1).Ticks;

        public CountDownService(IOptionsMonitor<WeddingOptions> settings)
        {
            _leftTicks = settings.CurrentValue.Date.Subtract(DateTime.Now).Ticks;
            _timer = new Timer(1000);
            _timer.Elapsed += CountDownTimer;
            _timer.Enabled = true;
        }

        public (string, string, string, string) GetLeftDate() => (Days, Hours, Minutes, Seconds);

        private void CountDownTimer(object source, ElapsedEventArgs e)
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
        }
    }
}