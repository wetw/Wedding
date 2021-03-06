﻿using System;

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

    }
}