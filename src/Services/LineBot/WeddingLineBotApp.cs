using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Enums;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using Wedding.Data;
using Wedding.Data.ReplyIntent;

namespace Wedding.Services.LineBot
{
    public class WeddingLineBotApp : LineBotApp
    {
        private readonly IOptionsMonitor<LineBotSetting> _settings;
        private readonly OnBeaconIntent _onBeaconIntent;
        private readonly OnFollowIntent _onFollowIntent;
        private readonly OnMessageIntent _onMessageIntent;
        private readonly OnPostbackIntent _onPostbackIntent;

        public WeddingLineBotApp(
            ILineMessageUtility lineMessageUtility,
            IOptionsMonitor<LineBotSetting> settings,
            OnBeaconIntent onBeaconIntent,
            OnFollowIntent onFollowIntent,
            OnMessageIntent onMessageIntent,
            OnPostbackIntent onPostbackIntent) : base(lineMessageUtility)
        {
            _settings = settings;
            _onBeaconIntent = onBeaconIntent;
            _onFollowIntent = onFollowIntent;
            _onMessageIntent = onMessageIntent;
            _onPostbackIntent = onPostbackIntent;
        }

        protected override async Task OnFollowAsync(LineEvent ev)
        {
            await _onFollowIntent.ReplyAsync(ev).ConfigureAwait(false);
        }

        protected override Task OnMessageAsync(LineEvent ev) =>
            ev.message.Type == LineMessageType.Text
                ? _onMessageIntent.ReplyAsync(ev)
                : Task.CompletedTask;

        protected override Task OnBeaconAsync(LineEvent ev)
        {
            if (_settings.CurrentValue.Beacon.Enabled && ev.beacon.type == BeaconType.Enter)
            {
                return _onBeaconIntent.ReplyAsync(ev);
            }

            return Task.CompletedTask;
        }

        protected override Task OnPostbackAsync(LineEvent ev) =>
            _onPostbackIntent.ReplyAsync(ev);
    }
}