using System;

namespace ModIso8583.Parse
{
    public class Date10ParseInfo : DateTimeParseInfo
    {
        public Date10ParseInfo() : base(IsoType.DATE10,
            10)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0)
                throw new Exception($"Invalid DATE10 field {field} position {pos}");
            if (pos + 10 > buf.Length)
                throw new Exception($"Insufficient data for DATE10 field {field}, pos {pos}");


            DateTimeOffset calendar;
            if (ForceStringDecoding)
                calendar = new DateTimeOffset(DateTime.Now.Year,
                    int.Parse(Encoding.GetString(buf,
                        pos,
                        2)) - 1,
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
            else
                calendar = new DateTimeOffset(DateTime.Now.Year,
                    (buf[pos] - 48) * 10 + buf[pos + 1] - 49,
                    (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48,
                    (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 48,
                    (buf[pos + 6] - 48) * 10 + buf[pos + 7] - 48,
                    (buf[pos + 8] - 48) * 10 + buf[pos + 9] - 48,
                    0,
                    TimeSpan.Zero);

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
            throw new NotImplementedException();
        }
    }
}