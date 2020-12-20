using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Enums;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using System.Threading.Tasks;

namespace Wedding.Services.LineBot
{
    public class WeddingLineBotApp : LineBotApp
    {
        private readonly ILineMessageUtility _lineMessageUtility;

        public WeddingLineBotApp(ILineMessageUtility lineMessageUtility) : base(lineMessageUtility)
        {
            _lineMessageUtility = lineMessageUtility;
        }

        protected override async Task OnMessageAsync(LineEvent ev)
        {
            await _lineMessageUtility.ReplyMessageAsync(ev.replyToken, $"You Said:{ev.message.Text}");
        }

        protected override async Task OnBeaconAsync(LineEvent ev)
        {
            if (ev.beacon.type == BeaconType.enter)
            {
                var userProfile= await _lineMessageUtility.GetUserProfile(ev.source.userId).ConfigureAwait(false);
                await _lineMessageUtility.ReplyMessageAsync(ev.replyToken, $"歡迎蒞臨\r\n{userProfile.displayName}").ConfigureAwait(false);
            }
        }
    }
}