using System.Threading.Tasks;
using NetCoreLineBotSDK.Models.LineObject;

namespace Wedding.Data.ReplyIntent
{
    public interface IReplyIntent
    {
        Task ReplyAsync(LineEvent ev);
    }
}