using System;

namespace ModIso8583.Parse
{
    public class Date12ParseInfo : DateTimeParseInfo
    {
        public Date12ParseInfo() : base(IsoType.DATE12,
            12)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE12 field {field} position {pos}");
            if (pos + 12 > buf.Length) throw new Exception($"Insufficient data for DATE12 field {field}, pos {pos}");

            DateTimeOffset calendar;
            int year;
            if (ForceStringDecoding)
            {
                year = int.Parse(Encoding.GetString(buf,
                    pos,
                    2));
                if (year > 50) year = 1900 + year;
                else year = 2000 + year;
                calendar = new DateTimeOffset(year,
                    int.Parse(Encoding.GetString(buf,
                        pos,
                        2)),
                    int.Parse(Encoding.GetString(buf,
                        pos + 2,
                        2)),
                    int.Parse(Encoding.GetString(buf,
                        pos + 4,
                        2)),
                    int.Parse(Encoding.GetString(buf,
                        pos + 6,
                        2)),
                    int.Parse(Encoding.GetString(buf,
                        pos + 8,
                        2)),
                    0,
                    TimeSpan.Zero);
            }
            else
            {
                year = (buf[pos] - 48) * 10 + buf[pos + 1] - 48;
                if (year > 50) year = 1900 + year;
                else year = 2000 + year;
                calendar = new DateTimeOffset(year,
                    (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 49,
                    (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 48,
                    (buf[pos + 6] - 48) * 10 + buf[pos + 7] - 48,
                    (buf[pos + 8] - 48) * 10 + buf[pos + 9] - 48,
                    (buf[pos + 10] - 48) * 10 + buf[pos + 11] - 48,
                    0,
                    TimeSpan.Zero);
            }

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            calendar = AdjustWithFutureTolerance(calendar);
            return new IsoValue(IsoType,
                calendar.DateTime);
        }

        public override IsoValue ParseBinary(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE12 field {field} position {pos}");
            if (pos + 6 > buf.Length) throw new Exception($"Insufficient data for DATE12 field {field}, pos {pos}");

            var tens = new int[6];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            int year;
            if (tens[0] > 50) year = 1900 + tens[0];
            else year = 2000 + tens[0];
            var calendar = new DateTimeOffset(year,
                tens[1] - 1,
                tens[2],
                tens[3],
                tens[4],
                tens[5],
                0,
                TimeSpan.Zero);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            calendar = AdjustWithFutureTolerance(calendar);
            return new IsoValue(IsoType,
                calendar.DateTime);
        }
    }
}