using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;

namespace Wedding.Data.ReplyIntent
{
    public class OnBeaconIntent : AbstractReplyIntent
    {
        private readonly IOptionsMonitor<LineBotSetting> _settings;
        public OnBeaconIntent(
            ILineMessageUtility lineMessageUtility,
            IFileProvider fileProvider,
            IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility, fileProvider)
        {
            _settings = settings;
        }

        public override Task ReplyAsync(LineEvent ev) =>
            TryGetTemplateMessageAsync(ev, _settings.CurrentValue.AdvanceReplyMapping.OnBeacon);
    }
}