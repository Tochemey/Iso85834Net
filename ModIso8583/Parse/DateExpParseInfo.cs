using System;

namespace ModIso8583.Parse
{
    public class DateExpParseInfo : DateTimeParseInfo
    {
        public DateExpParseInfo() : base(IsoType.DATE_EXP,
            4)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE_EXP field {field} position {pos}");
            if (pos + 4 > buf.Length) throw new Exception($"Insufficient data for DATE_EXP field {field}, pos {pos}");

            var calendar = new DateTime();
            calendar = calendar.AddHours(0).AddMinutes(0).AddSeconds(0).AddDays(1);
            if (ForceStringDecoding)
            {
                calendar = calendar.AddYears(calendar.Year - calendar.Year % 100 + int.Parse(Encoding.GetString(buf,
                                                 pos,
                                                 2)));
                calendar = calendar.AddMonths(int.Parse(Encoding.GetString(buf,
                    pos + 2,
                    2)));
            }
            else
            {
                calendar = calendar.AddYears(calendar.Year - calendar.Year % 100 + (buf[pos] - 48) * 10 + buf[pos + 1] - 48);
                calendar = calendar.AddMonths((buf[pos + 2] - 48) * 10 + buf[pos + 3] - 49);
            }

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
            if (pos < 0) throw new Exception($"Invalid DATE_EXP field {field} position {pos}");
            if (pos + 2 > buf.Length) throw new Exception($"Insufficient data for DATE_EXP field {field} pos {pos}");

            var tens = new int[2];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            var calendar = new DateTime(DateTime.Now.Year - DateTime.Now.Year % 100 + tens[0],
                tens[1] - 1,
                1,
                0,
                0,
                0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            var ajusted = AdjustWithFutureTolerance(new DateTimeOffset(calendar));
            return new IsoValue(IsoType,
                ajusted.DateTime);
        }
    }
}