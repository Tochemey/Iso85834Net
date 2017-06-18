﻿using System;

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
            if (pos < 0) throw new Exception($"Invalid DATE10 field {field} position {pos}");
            if (pos + 10 > buf.Length) throw new Exception($"Insufficient data for DATE10 field {field}, pos {pos}");


            DateTime calendar;
            if (ForceStringDecoding)
                calendar = new DateTime(DateTime.Now.Year,
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
                        2)));
            else
                calendar = new DateTime(DateTime.Now.Year,
                    (buf[pos] - 48) * 10 + buf[pos + 1] - 49,
                    (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48,
                    (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 48,
                    (buf[pos + 6] - 48) * 10 + buf[pos + 7] - 48,
                    (buf[pos + 8] - 48) * 10 + buf[pos + 9] - 48);

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
            if (pos < 0) throw new Exception($"Invalid DATE10 field {field} position {pos}");
            if (pos + 5 > buf.Length) throw new Exception($"Insufficient data for DATE10 field {field}, pos {pos}");

            var tens = new int[5];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            var calendar = new DateTime(DateTime.Now.Year,
                tens[0] - 1,
                tens[1],
                tens[2],
                tens[3],
                tens[4]).AddMilliseconds(0);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            var ajusted = AdjustWithFutureTolerance(new DateTimeOffset(calendar));
            return new IsoValue(IsoType,
                ajusted.DateTime);
        }
    }
}