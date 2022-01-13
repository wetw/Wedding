using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;

namespace Wedding.Data
{
    public partial class LineBotSetting
    {
        public Beacon Beacon { get; init; }

        public string ClientId { get; init; }

        public string ClientSecret { get; init; }

        public Dictionary<string, string> MessageReplyMapping { get; init; }

        public Dictionary<string, string> PostbackReplyMapping { get; init; }

        public AdvanceReplyMapping AdvanceReplyMapping { get; init; }

        public string OnFollowTextMessage { get; init; }
    }

    public partial class Beacon
    {
        public bool Enabled { get; init; }

        public bool EnabledAlwaysSendTextMessage { get; init; }

        public string OnBeaconTextMessage { get; init; }
    }

    public class AdvanceReplyMapping
    {
        public ReplyObject OnBeacon { get; init; }

        public ReplyObject OnFollow { get; init; }

        public Dictionary<string, ReplyObject> OnMessage { get; init; }

        public Dictionary<string, ReplyObject> OnPostback { get; init; }
    }

    public class ReplyObject
    {
        public ReplyType Type { get; init; } = ReplyType.Continue;

        public int RandomNum { get; init; } = 1;

        public IEnumerable<string> Templates { get; init; }
    }
}
