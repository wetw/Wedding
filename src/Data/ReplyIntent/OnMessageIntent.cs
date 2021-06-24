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
            if (_settings.CurrentValue.MessageReplyMapping.TryGetValue(ev.message.Text, out var value))
            {
                return TryGetTemplateMessageAsync(ev, value, TemplateFolderPath);
            }

            // If not match, will try to fuzzy search
            foreach (var result in _settings.CurrentValue.MessageReplyMapping
                .Where(x => ev.message.Text.Contains(x.Key)))
            {
                return TryGetTemplateMessageAsync(ev, result.Value, TemplateFolderPath);
            }
            return Task.CompletedTask;
        }
    }
}