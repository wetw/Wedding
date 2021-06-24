using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using NetCoreLineBotSDK.Models.Message;

namespace Wedding.Data.ReplyIntent
{
    public class OnFollowIntent : AbstractReplyIntent
    {
        private readonly IOptionsMonitor<LineBotSetting> _settings;
        private const string TemplateFolderPath = "IntentMessages/OnFollow";
        public OnFollowIntent(
            ILineMessageUtility lineMessageUtility,
            IFileProvider fileProvider,
            IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility, fileProvider)
        {
            _settings = settings;
        }

        public override async Task ReplyAsync(LineEvent ev)
        {
            var messages = await GetTemplateMessages(TemplateFolderPath).ConfigureAwait(false);

            if (messages.Count == 0)
            {
                var userProfile = await LineMessageUtility.GetUserProfile(ev.source.userId).ConfigureAwait(false);
                messages.Add(new TextMessage(string.Format(_settings.CurrentValue.OnFollowTextMessage, userProfile.displayName)));
            }

            await LineMessageUtility.ReplyMessageAsync(ev.replyToken, messages).ConfigureAwait(false);
        }
    }
}
