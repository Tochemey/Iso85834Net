using System;

namespace ModIso8583.Parse
{
    public class Date14ParseInfo : DateTimeParseInfo
    {
        public Date14ParseInfo() : base(IsoType.DATE14,
            14)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE14 field {field} position {pos}");
            if (pos + 14 > buf.Length) throw new Exception($"Insufficient data for DATE14 field {field}, pos {pos}");

            DateTime calendar;
            if (ForceStringDecoding)
                calendar = new DateTime(int.Parse(Encoding.GetString(buf,
                        pos,
                        4)),
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
                        2)));
            else
                calendar = new DateTime((buf[pos] - 48) * 1000 + (buf[pos + 1] - 48) * 100 + (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48,
                    (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 49,
                    (buf[pos + 6] - 48) * 10 + buf[pos + 7] - 48,
                    (buf[pos + 8] - 48) * 10 + buf[pos + 9] - 48,
                    (buf[pos + 10] - 48) * 10 + buf[pos + 11] - 48,
                    (buf[pos + 12] - 48) * 10 + buf[pos + 13] - 48);
            calendar.AddMilliseconds(0);
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
            if (pos < 0) throw new Exception($"Invalid DATE14 field {field} position {pos}");
            if (pos + 7 > buf.Length) throw new Exception($"Insufficient data for DATE14 field {field}, pos {pos}");

            var tens = new int[7];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            var calendar = new DateTime(tens[0] * 100 + tens[1],
                tens[2] - 1,
                tens[3],
                tens[4],
                tens[5],
                tens[6]).AddMilliseconds(0);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            var ajusted = AdjustWithFutureTolerance(new DateTimeOffset(calendar));
            return new IsoValue(IsoType,
                ajusted.DateTime);
        }
    }
}