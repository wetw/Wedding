using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuzzyString;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;

namespace Wedding.Data.ReplyIntent
{
    public class OnMessageIntent : AbstractReplyIntent
    {
        private readonly IOptionsMonitor<LineBotSetting> _settings;
        private readonly IList<FuzzyStringComparisonOptions> _options;
        public OnMessageIntent(
            ILineMessageUtility lineMessageUtility,
            IFileProvider fileProvider,
            IOptionsMonitor<LineBotSetting> settings) : base(lineMessageUtility, fileProvider)
        {
            _settings = settings;
            _options = new List<FuzzyStringComparisonOptions>
            {
                FuzzyStringComparisonOptions.UseJaccardDistance,
                FuzzyStringComparisonOptions.UseNormalizedLevenshteinDistance,
                FuzzyStringComparisonOptions.UseOverlapCoefficient,
                FuzzyStringComparisonOptions.UseLongestCommonSubsequence,
                FuzzyStringComparisonOptions.CaseSensitive
            };
        }

        public override Task ReplyAsync(LineEvent ev)
        {
            var replyObjects = _settings.CurrentValue.AdvanceReplyMapping.OnMessage;
            var text = ev.message.Text.Trim();

            // fuzzy search
            var key = replyObjects.Keys.FirstOrDefault(x => x.ApproximatelyEquals(text, FuzzyStringComparisonTolerance.Normal, _options.ToArray()));
            if (key is not null && replyObjects.TryGetValue(key, out var fuzzyReplyObj))
            {
                return TryGetTemplateMessageAsync(ev, fuzzyReplyObj);
            }

            return replyObjects.TryGetValue("*", out var defaultReplyObj)
                ? TryGetTemplateMessageAsync(ev, defaultReplyObj)
                : Task.CompletedTask;
        }
    }
}