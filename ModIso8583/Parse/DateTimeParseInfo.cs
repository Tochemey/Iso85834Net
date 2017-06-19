using System;
using ModIso8583.Util;

namespace ModIso8583.Parse
{
    public abstract class DateTimeParseInfo : FieldParseInfo
    {
        protected static readonly long FUTURE_TOLERANCE = 900000L;
        public TimeZoneInfo TimeZoneInfo { get; set; }

        public DateTimeParseInfo(IsoType isoType,
            int length) : base(isoType,
            length)
        { }

        public static DateTimeOffset AdjustWithFutureTolerance(DateTimeOffset calendar)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long then = calendar.ToUnixTimeMilliseconds();
            if (then > now && then - now > FUTURE_TOLERANCE) return calendar.AddYears(-1);
            return calendar;
        }
    }
}