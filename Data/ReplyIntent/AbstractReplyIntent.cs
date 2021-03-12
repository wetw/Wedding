using System.Threading.Tasks;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;

namespace Wedding.Data.ReplyIntent
{
    public abstract class AbstractReplyIntent : IReplyIntent
    {
        private protected static ILineMessageUtility LineMessageUtility;

        protected AbstractReplyIntent(ILineMessageUtility lineMessageUtility)
        {
            LineMessageUtility = lineMessageUtility;
        }

        public abstract Task ReplyAsync(LineEvent ev);
    }
}