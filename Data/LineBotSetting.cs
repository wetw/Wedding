﻿namespace Wedding.Data
{
    public partial class LineBotSetting
    {
        public Beacon Beacon { get; set; }

        public string LiffClientId { get; set; }
    }

    public partial class Beacon
    {
        public bool Enabled { get; set; }
    }
}
