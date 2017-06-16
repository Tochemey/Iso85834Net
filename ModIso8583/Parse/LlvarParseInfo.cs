using System;

namespace ModIso8583.Parse
{
    public class LlvarParseInfo : FieldParseInfo
    {
        public LlvarParseInfo() : base(IsoType.LLVAR,
            0)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid LLVAR field {field} {pos}");
            if (pos + 2 > buf.Length) throw new Exception($"Insufficient data for LLVAR header, pos {pos}");
            var len = DecodeLength(buf,
                pos,
                2);
            if (len < 0) throw new Exception($"Invalid LLVAR length {len}, field {field} pos {pos}");
            if (len + pos + 2 > buf.Length) throw new Exception($"Insufficient data for LLVAR field {field}, pos {pos}");

            string v;
            try
            {
                v = len == 0 ? "" : Encoding.GetString(buf,
                    pos + 2,
                    len);
            }
            catch (Exception) { throw new Exception($"Insufficient data for LLVAR header, field {field} pos {pos}"); }
            //This is new: if the String's length is different from the specified length in the
            //buffer, there are probably some extended characters. So we create a String from
            //the rest of the buffer, and then cut it to the specified length.
            if (v.Length != len)
                v = Encoding.GetString(buf,
                    pos + 3,
                    buf.Length - pos - 3).Substring(0,
                    len);
            if (custom == null)
                return new IsoValue(IsoType,
                    v,
                    len);
            var decoded = custom.DecodeField(v);
            //If decode fails, return string; otherwise use the decoded object and its codec
            return decoded == null ? new IsoValue(IsoType,
                v,
                len) : new IsoValue(IsoType,
                decoded,
                len,
                custom);
        }

        public override IsoValue ParseBinary(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid bin LLVAR field {field} pos {pos}");
            if (pos + 1 > buf.Length) throw new Exception($"Insufficient data for bin LLVAR header, field {field} pos {pos}");
            var len = ((buf[pos] & 0xf0) >> 4) * 10 + (buf[pos] & 0x0f);
            if (len < 0) throw new Exception($"Invalid bin LLVAR length {len}, field {field} pos {pos}");
            if (len + pos + 1 > buf.Length) throw new Exception($"Insufficient data for bin LLVAR field {field}, pos {pos}");

            if (custom == null)
            {
                return new IsoValue(IsoType,
                    Encoding.GetString(buf,
                        pos + 1,
                        len));
            }
            var dec = custom.DecodeField(Encoding.GetString(buf,
                pos + 1,
                len));
            return dec == null ? new IsoValue(IsoType,
                Encoding.GetString(buf,
                    pos + 1,
                    len)) : new IsoValue(IsoType,
                dec,
                custom);
        }
    }
}