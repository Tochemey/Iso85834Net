using System;
using ModIso8583.Util;

namespace ModIso8583.Parse
{
    public class Date14ParseInfo : DateTimeParseInfo
    {
        public Date14ParseInfo() : base(IsoType.DATE14,
            14)
        { }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE14 field {field} position {pos}");
            if (pos + 14 > buf.Length) throw new Exception($"Insufficient data for DATE14 field {field}, pos {pos}");

            DateTime calendar;
            if (ForceStringDecoding)
            {
                var year = Convert.ToInt32(buf.SbyteString(pos,
                        4,
                        Encoding),
                    10);
                var month = Convert.ToInt32(buf.SbyteString(pos,
                        2,
                        Encoding),
                    10);
                var day = Convert.ToInt32(buf.SbyteString(pos + 2,
                        2,
                        Encoding),
                    10);
                var hour = Convert.ToInt32(buf.SbyteString(pos + 4,
                        2,
                        Encoding),
                    10);
                var min = Convert.ToInt32(buf.SbyteString(pos + 6,
                        2,
                        Encoding),
                    10);
                var sec = Convert.ToInt32(buf.SbyteString(pos + 8,
                        2,
                        Encoding),
                    10);

                //calendar = new DateTime(int.Parse(Encoding.GetString(buf,
                //        pos,
                //        4)),
                //    int.Parse(Encoding.GetString(buf,
                //        pos,
                //        2)),
                //    int.Parse(Encoding.GetString(buf,
                //        pos + 2,
                //        2)),
                //    int.Parse(Encoding.GetString(buf,
                //        pos + 4,
                //        2)),
                //    int.Parse(Encoding.GetString(buf,
                //        pos + 6,
                //        2)),
                //    int.Parse(Encoding.GetString(buf,
                //        pos + 8,
                //        2)));
                calendar = new DateTime(year,
                    month,
                    day,
                    hour,
                    min,
                    sec);
            }
            else
            {
                var sbytes = buf;
                calendar = new DateTime((sbytes[pos] - 48) * 1000 + (sbytes[pos + 1] - 48) * 100 + (sbytes[pos + 2] - 48) * 10 + sbytes[pos + 3] - 48,
                    (sbytes[pos + 4] - 48) * 10 + sbytes[pos + 5] - 48,
                    (sbytes[pos + 6] - 48) * 10 + sbytes[pos + 7] - 48,
                    (sbytes[pos + 8] - 48) * 10 + sbytes[pos + 9] - 48,
                    (sbytes[pos + 10] - 48) * 10 + sbytes[pos + 11] - 48,
                    (sbytes[pos + 12] - 48) * 10 + sbytes[pos + 13] - 48);
            }

            calendar = calendar.AddMilliseconds(0);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            var ajusted = AdjustWithFutureTolerance(new DateTimeOffset(calendar));

            return new IsoValue(IsoType,
                ajusted.DateTime);
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE14 field {field} position {pos}");
            if (pos + 7 > buf.Length) throw new Exception($"Insufficient data for DATE14 field {field}, pos {pos}");
            var sbytes = buf;
            var tens = new int[7];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((sbytes[i] & 0xf0) >> 4) * 10 + (sbytes[i] & 0x0f);

            var calendar = new DateTime(tens[0] * 100 + tens[1],
                tens[2],
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