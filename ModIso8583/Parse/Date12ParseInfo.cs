using System;
using ModIso8583.Util;

namespace ModIso8583.Parse
{
    public class Date12ParseInfo : DateTimeParseInfo
    {
        public Date12ParseInfo() : base(IsoType.DATE12,
            12)
        { }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE12 field {field} position {pos}");
            if (pos + 12 > buf.Length) throw new Exception($"Insufficient data for DATE12 field {field}, pos {pos}");

            DateTime calendar;
            int year;
            if (ForceStringDecoding)
            {
                //year = int.Parse(Encoding.GetString(buf,
                //    pos,
                //    2));
                year = Convert.ToInt32(buf.SbyteString(pos,
                        2,
                        Encoding),
                    10);

                if (year > 50) year = 1900 + year;
                else year = 2000 + year;
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
                calendar = new DateTime(year,
                    month,
                    day,
                    hour,
                    min,
                    sec);

                //calendar = new DateTime(year,
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
            }
            else
            {
                var sbytes = buf;
                year = (sbytes[pos] - 48) * 10 + sbytes[pos + 1] - 48;
                if (year > 50) year = 1900 + year;
                else year = 2000 + year;
                calendar = new DateTime(year,
                    (sbytes[pos + 2] - 48) * 10 + sbytes[pos + 3] - 48,
                    (sbytes[pos + 4] - 48) * 10 + sbytes[pos + 5] - 48,
                    (sbytes[pos + 6] - 48) * 10 + sbytes[pos + 7] - 48,
                    (sbytes[pos + 8] - 48) * 10 + sbytes[pos + 9] - 48,
                    (sbytes[pos + 10] - 48) * 10 + sbytes[pos + 11] - 48);
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
            if (pos < 0) throw new Exception($"Invalid DATE12 field {field} position {pos}");
            if (pos + 6 > buf.Length) throw new Exception($"Insufficient data for DATE12 field {field}, pos {pos}");

            var tens = new int[6];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            int year;
            if (tens[0] > 50) year = 1900 + tens[0];
            else year = 2000 + tens[0];
            var calendar = new DateTime(year,
                tens[1],
                tens[2],
                tens[3],
                tens[4],
                tens[5]).AddMilliseconds(0);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            var ajusted = AdjustWithFutureTolerance(new DateTimeOffset(calendar));
            return new IsoValue(IsoType,
                ajusted.DateTime);
        }
    }
}