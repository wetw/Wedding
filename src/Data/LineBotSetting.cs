using System.Collections.Generic;

namespace Wedding.Data
{
    public partial class LineBotSetting
    {
        public Beacon Beacon { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public Dictionary<string, string> MessageReplyMapping { get; init; }

        public Dictionary<string, string> PostbackReplyMapping { get; init; }

        public string OnFollowTextMessage { get; init; }
    }

    public partial class Beacon
    {
        public bool Enabled { get; set; }

        public bool EnabledAlwaysSendTextMessage { get; init; }

        public string OnBeaconTextMessage { get; init; }
    }
}
