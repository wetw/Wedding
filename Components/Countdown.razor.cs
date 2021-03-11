using System;
using Microsoft.AspNetCore.Components;
using System.Timers;
using Wedding.Services;

namespace Wedding.Components
{
    public partial class Countdown : IDisposable
    {
        [Inject]
        private CountDownService CountDownService { get; init; }

        [Parameter]
        public string Id { get; set; } = "countdown";

        [Parameter]
        public string Style { get; set; } = "";

        private string Days { get; set; } = "000";
        private string Hours { get; set; } = "00";
        private string Minutes { get; set; } = "00";
        private string Seconds { get; set; } = "00";
        private static Timer _timer;

        protected override void OnInitialized()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += CountDownTimer;
            _timer.Enabled = true;
            _timer.Start();
        }

        private void CountDownTimer(object source, ElapsedEventArgs e)
        {
            _ = InvokeAsync(() =>
            {
                (Days, Hours, Minutes, Seconds) = CountDownService.GetLeftDate();
                StateHasChanged();
            });
        }

        void IDisposable.Dispose()
        {
            _timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
