using System.Linq;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Enums;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using System.Threading.Tasks;
using Wedding.Data;
using Wedding.Data.ReplyIntent;

namespace Wedding.Services.LineBot
{
    public class WeddingLineBotApp : LineBotApp
    {
        private readonly ILineMessageUtility _lineMessageUtility;
        private readonly IOptionsMonitor<LineBotSetting> _settings;
        private readonly BeaconWelcomeIntent _beaconWelcomeIntent;

        public WeddingLineBotApp(ILineMessageUtility lineMessageUtility,
            IOptionsMonitor<LineBotSetting> settings,
            BeaconWelcomeIntent beaconWelcomeIntent) : base(lineMessageUtility)
        {
            _lineMessageUtility = lineMessageUtility;
            _settings = settings;
            _beaconWelcomeIntent = beaconWelcomeIntent;
        }

        protected override async Task OnMessageAsync(LineEvent ev)
        {
            if (ev.message.Type == LineMessageType.text)
            {
                foreach (var message in _settings.CurrentValue.MessageReplyMapping
                    .Where(message => ev.message.Text.Contains(message.Key)))
                {
                    await _lineMessageUtility.ReplyMessageAsync(ev.replyToken, message.Value).ConfigureAwait(false);
                }
            }
        }

        protected override async Task OnBeaconAsync(LineEvent ev)
        {
            if (_settings.CurrentValue.Beacon.Enabled && ev.beacon.type == BeaconType.enter)
            {
                await _beaconWelcomeIntent.ReplyAsync(ev).ConfigureAwait(false);
            }
        }
    }
}