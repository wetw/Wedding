using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;

namespace Wedding.Data.ReplyIntent
{
    public class OnMessageIntent : AbstractReplyIntent
    {
        private const string TemplateFolderPath = "IntentMessages/OnMessage";
        private readonly IOptionsMonitor<LineBotSetting> _settings;

        public OnMessageIntent(
            ILineMessageUtility lineMessageUtility,
            IFileProvider fileProvider,
            IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility, fileProvider)
        {
            _settings = settings;
        }

        public override Task ReplyAsync(LineEvent ev)
        {
            var currentMapping = _settings.CurrentValue.AdvanceReplyMapping;
            if (currentMapping.OnMessage.TryGetValue(ev.message.Text.Trim(), out var replyObject))
            {
                return TryGetTemplateMessageAsync(ev, replyObject);
            }

            // If not match, will try to fuzzy search
            foreach (var result in currentMapping.OnMessage.Where(x => x.Key.Contains(ev.message.Text.Trim())))
            {
                return TryGetTemplateMessageAsync(ev, result.Value);
            }

            return Task.CompletedTask;
        }
    }
}