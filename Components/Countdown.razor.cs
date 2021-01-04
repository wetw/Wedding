using Microsoft.AspNetCore.Components;
using System.Timers;
using Wedding.Services;

namespace Wedding.Components
{
    public partial class Countdown
    {
        [Inject]
        private CountDownService CountDownService { get; init; }

        [Parameter]
        public string Id { get; set; } = "countdown";

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
        }

        private async void CountDownTimer(object source, ElapsedEventArgs e)
        {
            (Days, Hours, Minutes, Seconds) = CountDownService.GetLeftDate();
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        }
    }
}
