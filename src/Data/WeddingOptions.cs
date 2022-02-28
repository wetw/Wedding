using System;
using System.Collections.Generic;

namespace Wedding.Data
{
    public class WeddingOptions
    {
        public string Name { get; init; }

        public DateTime Date { get; init; }

        public string CultureInfo { get; init; }

        public string DayFormat { get; init; }

        public string TimeFormat { get; init; }

        public string InvitationDescription { get; init; }

        public string WeddingVideoSrc { get; init; }

        public float BulletScaling { get; init; }

        public int LuckyManCount { get; init; } = 3;

        public bool PushMessageToLuckyMan { get; init; }

        public string RandomKey { get; init; } = "123456";

        public int PhotoBlessingScannerTime { get; init; } = 1000;
    }
}