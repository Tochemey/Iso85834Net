using System;
using ModIso8583.Util;

namespace ModIso8583.Parse
{
    public class TimeParseInfo : DateTimeParseInfo
    {
        public TimeParseInfo() : base(IsoType.TIME,
            6)
        { }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid TIME field {field} pos {pos}");
            if (pos + 6 > buf.Length) throw new Exception($"Insufficient data for TIME field {field}, pos {pos}");
            TimeSpan calendar;
            if (ForceStringDecoding)
            {
                calendar = new TimeSpan(DateTime.Now.Day,
                    Convert.ToInt32(buf.SbyteString(pos,
                            2,
                            Encoding),
                        10),
                    Convert.ToInt32(buf.SbyteString(pos + 2,
                            2,
                            Encoding),
                        10),
                    Convert.ToInt32(buf.SbyteString(pos + 4,
                        2,
                        Encoding)),
                    10);
            }
            else
            {
                var sbytes = buf;
                calendar = new TimeSpan(DateTime.Now.Day,
                    (sbytes[pos] - 48) * 10 + sbytes[pos + 1] - 48,
                    (sbytes[pos + 2] - 48) * 10 + sbytes[pos + 3] - 48,
                    (sbytes[pos + 4] - 48) * 10 + sbytes[pos + 5] - 48);
            }

            //todo timespan to timezone
            return new IsoValue(IsoType,
                calendar);
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid bin TIME field {field} pos {pos}");
            if (pos + 3 > buf.Length) throw new Exception($"Insufficient data for bin TIME field {field}, pos {pos}");
            var sbytes = buf;
            var tens = new int[3];
            var start = 0;
            for (var i = pos; i < pos + 3; i++) tens[start++] = ((sbytes[i] & 0xf0) >> 4) * 10 + (sbytes[i] & 0x0f);

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