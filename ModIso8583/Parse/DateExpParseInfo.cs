using System;
using ModIso8583.Util;

namespace ModIso8583.Parse
{
    public class DateExpParseInfo : DateTimeParseInfo
    {
        public DateExpParseInfo() : base(IsoType.DATE_EXP,
            4)
        { }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE_EXP field {field} position {pos}");
            if (pos + 4 > buf.Length) throw new Exception($"Insufficient data for DATE_EXP field {field}, pos {pos}");

            int year, month;
            int hour, minute, seconds;
            hour = minute = seconds = 0;
            var day = 1;

            var sbytes = buf;

            if (ForceStringDecoding)
            {
                //year = DateTime.Today.Year - (DateTime.Today.Year % 100) + Convert.ToInt32(Encoding.GetString(buf,
                //               pos,
                //               2),
                //           10);

                year = DateTime.Today.Year - DateTime.Today.Year % 100 + Convert.ToInt32(buf.SbyteString(pos,
                           2,
                           Encoding));

                //month = Convert.ToInt32(Encoding.GetString(buf,
                //        pos + 2,
                //        2),
                //    10);

                month = Convert.ToInt32(buf.SbyteString(pos + 2,
                    2,
                    Encoding));
            }
            else
            {
                year = DateTime.Today.Year - DateTime.Today.Year % 100 + (sbytes[pos] - 48) * 10 + sbytes[pos + 1] - 48;
                month = (sbytes[pos + 2] - 48) * 10 + sbytes[pos + 3] - 48;
            }

            var calendar = new DateTime(year,
                month,
                day,
                hour,
                minute,
                seconds);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                calendar);
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid DATE_EXP field {field} position {pos}");
            if (pos + 2 > buf.Length) throw new Exception($"Insufficient data for DATE_EXP field {field} pos {pos}");

            var tens = new int[2];
            var start = 0;
            var sbytes = buf;

            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((sbytes[i] & 0xf0) >> 4) * 10 + (sbytes[i] & 0x0f);

            var calendar = new DateTime(DateTime.Now.Year - DateTime.Now.Year % 100 + tens[0],
                tens[1],
                1,
                0,
                0,
                0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                calendar);
        }
    }
}