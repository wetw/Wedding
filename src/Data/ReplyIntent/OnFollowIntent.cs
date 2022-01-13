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
        public OnFollowIntent(
            ILineMessageUtility lineMessageUtility,
            IFileProvider fileProvider,
            IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility, fileProvider)
        {
            _settings = settings;
        }

        public override Task ReplyAsync(LineEvent ev) =>
            TryGetTemplateMessageAsync(ev, _settings.CurrentValue.AdvanceReplyMapping.OnFollow);
    }
}
