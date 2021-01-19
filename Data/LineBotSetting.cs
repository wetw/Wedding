﻿namespace Wedding.Data
{
    public partial class LineBotSetting
    {
        public Beacon Beacon { get; set; }

        public string ClientId { get; set; }
        
        public string ClientSecret { get; set; }
    }

    public partial class Beacon
    {
        public bool Enabled { get; set; }
    }
}
