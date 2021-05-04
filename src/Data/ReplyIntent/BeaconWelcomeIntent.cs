using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using NetCoreLineBotSDK.Models.Message;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wedding.Data.ReplyIntent
{
    public class BeaconWelcomeIntent : AbstractReplyIntent
    {
        private readonly IOptionsMonitor<LineBotSetting> _settings;

        public BeaconWelcomeIntent(ILineMessageUtility lineMessageUtility, IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility)
        {
            _settings = settings;
        }

        public override async Task ReplyAsync(LineEvent ev)
        {
            var imageMessage = new ImageMessage
            {
                OriginalContentUrl = "https://i.imgur.com/Li0UUul.png",
                PreviewImageUrl = "https://i.imgur.com/Li0UUul.png"
            };
            var userProfile = await LineMessageUtility.GetUserProfile(ev.source.userId).ConfigureAwait(false);
            var textMessage = new TextMessage
            {
                Text = string.Format(_settings.CurrentValue.OnBeaconWelcomeMessage, userProfile.displayName)
            };
            await LineMessageUtility.ReplyMessageAsync(
                ev.replyToken,
                new List<IMessage>
                {
                    imageMessage,
                    textMessage
                }).ConfigureAwait(false);
        }
    }
}