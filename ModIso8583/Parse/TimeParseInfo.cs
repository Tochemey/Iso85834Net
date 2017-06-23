using System;

namespace ModIso8583.Parse
{
    public class TimeParseInfo : DateTimeParseInfo
    {
        public TimeParseInfo() : base(IsoType.TIME,
            6)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0)
                throw new Exception($"Invalid TIME field {field} pos {pos}");
            if (pos + 6 > buf.Length)
                throw new Exception($"Insufficient data for TIME field {field}, pos {pos}");
            TimeSpan calendar;
            if (ForceStringDecoding)
                calendar = new TimeSpan(DateTime.Now.Day,
                    Convert.ToInt32(Encoding.GetString(buf,
                        pos,
                        2), 10),
                    Convert.ToInt32(Encoding.GetString(buf,
                        pos + 2,
                        2), 10),
                    Convert.ToInt32(Encoding.GetString(buf,
                        pos + 4,
                        2)), 10);
            else
                calendar = new TimeSpan(DateTime.Now.Day,
                    (buf[pos] - 48) * 10 + buf[pos + 1] - 48,
                    (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48,
                    (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 48);

            //todo timespan to timezone
            return new IsoValue(IsoType,
                calendar);
        }

        public override IsoValue ParseBinary(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid bin TIME field {field} pos {pos}");
            if (pos + 3 > buf.Length) throw new Exception($"Insufficient data for bin TIME field {field}, pos {pos}");

            var tens = new int[3];
            var start = 0;
            for (var i = pos; i < pos + 3; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            var calendar = new TimeSpan(DateTime.Now.Day,
                tens[0],
                tens[1],
                tens[2]);
            //todo timespan to timezone
            return new IsoValue(IsoType,
                calendar);
        }
    }
}