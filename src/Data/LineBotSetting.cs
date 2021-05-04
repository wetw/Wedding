using System.Collections.Generic;
using System.IO;

namespace Wedding.Data
{
    public partial class LineBotSetting
    {
        public Beacon Beacon { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public Dictionary<string, string> MessageReplyMapping { get; init; }

        public string OnBeaconWelcomeMessage { get; init; }
    }

    public partial class Beacon
    {
        public bool Enabled { get; set; }
    }
}
