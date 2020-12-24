using Microsoft.Extensions.Options;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Enums;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using System.Threading.Tasks;
using Wedding.Data;

namespace Wedding.Services.LineBot
{
    public class WeddingLineBotApp : LineBotApp
    {
        private readonly ILineMessageUtility _lineMessageUtility;
        private readonly IOptionsMonitor<LineBotSetting> _settings;

        public WeddingLineBotApp(ILineMessageUtility lineMessageUtility, IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility)
        {
            _lineMessageUtility = lineMessageUtility;
            _settings = settings;
        }

        protected override async Task OnMessageAsync(LineEvent ev)
        {
            await _lineMessageUtility.ReplyMessageAsync(ev.replyToken, $"You Said:{ev.message.Text}");
        }

        protected override async Task OnBeaconAsync(LineEvent ev)
        {
            if (_settings.CurrentValue.Beacon.Enabled && ev.beacon.type == BeaconType.enter)
            {
                var userProfile= await _lineMessageUtility.GetUserProfile(ev.source.userId).ConfigureAwait(false);
                await _lineMessageUtility.ReplyMessageAsync(ev.replyToken, $"歡迎蒞臨\r\n{userProfile.displayName}").ConfigureAwait(false);
            }
        }
    }
}