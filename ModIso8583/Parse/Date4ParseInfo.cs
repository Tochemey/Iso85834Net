using System;

namespace ModIso8583.Parse
{
    /// <summary>
    ///     This class is used to parse fields of type DATE4.
    /// </summary>
    public class Date4ParseInfo : DateTimeParseInfo
    {
        public Date4ParseInfo() : base(IsoType.DATE4,
            4)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE4 field {field} position {pos}");
            if (pos + 4 > buf.Length) throw new Exception($"Insufficient data for DATE4 field {field}, pos {pos}");

            var calendar = new DateTime();
            calendar = calendar.AddYears(DateTime.Today.Year);
            
            //Set the month in the date
            if (ForceStringDecoding)
            {
                var c = Encoding.GetString(buf,
                    pos,
                    2);
                calendar = calendar.AddMonths(int.Parse(c) - 1);
                c = Encoding.GetString(buf,
                    pos + 2,
                    2);
                calendar = calendar.AddDays(int.Parse(c));
            }
            else
            {
                calendar = calendar.AddMonths((buf[pos] - 48) * 10 + buf[pos + 1] - 49);
                calendar = calendar.AddDays((buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48);
            }

            calendar = calendar.AddHours(0).AddMinutes(0).AddSeconds(0).AddMilliseconds(0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            var ajusted = AdjustWithFutureTolerance(new DateTimeOffset(calendar));
            return new IsoValue(IsoType,
                ajusted.DateTime);
        }

        public override IsoValue ParseBinary(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            var tens = new int[2];
            var start = 0;
            if (buf.Length - pos < 2) throw new Exception($"Insufficient data to parse binary DATE4 field {field} pos {pos}");
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            var calendar = new DateTime(DateTime.Now.Year,
                tens[0] - 1,
                tens[1],
                0,
                0,
                0
                ).AddMilliseconds(0);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);
            var ajusted = AdjustWithFutureTolerance(new DateTimeOffset(calendar));
            return new IsoValue(IsoType,
                ajusted.DateTime);
        }
    }
}