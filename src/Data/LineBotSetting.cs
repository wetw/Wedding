using System;
using System.Collections.Generic;


namespace Wedding.Data
{
    public partial class LineBotSetting
    {
        public Beacon Beacon { get; init; }

        public string ClientId { get; init; }

        public string ClientSecret { get; init; }

        public AdvanceReplyMapping AdvanceReplyMapping { get; init; }

        public CustomerMessage CustomerMessage { get; init; }
    }

    public partial class Beacon
    {
        public bool Enabled { get; init; }

        public bool EnabledAlwaysSendTextMessage { get; init; }

        public string OnBeaconTextMessage { get; init; }
    }

    public partial class CustomerMessage
    {
        public AttendMessage AttendMessage { get; init; }

        public WelcomeMessage WelcomeMessage { get; init; }
    }

    public partial class WelcomeMessage
    {
        public bool Enabled { get; init; }

        public bool EnabledDaily { get; init; }

        public TimeSpan WelcomeBeforeUtcTime { get; init; }
    }

    public partial class AttendMessage
    {
        public bool Enabled { get; init; }
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
