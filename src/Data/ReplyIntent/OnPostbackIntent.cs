using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;

namespace Wedding.Data.ReplyIntent
{
    public class OnPostbackIntent : AbstractReplyIntent
    {
        private readonly IOptionsMonitor<LineBotSetting> _settings;
        private const string TemplateFolderPath = "IntentMessages/OnPostback";

        public OnPostbackIntent(
            ILineMessageUtility lineMessageUtility,
            IFileProvider fileProvider,
            IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility, fileProvider)
        {
            _settings = settings;
        }

        public override Task ReplyAsync(LineEvent ev) =>
            _settings.CurrentValue.PostbackReplyMapping.TryGetValue(ev.postback.data, out var value)
                ? TryGetTemplateMessageAsync(ev, value, TemplateFolderPath)
                : Task.CompletedTask;
    }
}