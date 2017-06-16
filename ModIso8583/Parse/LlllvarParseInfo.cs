using System;
using System.Text;

namespace ModIso8583.Parse
{
    public class LlllvarParseInfo : FieldParseInfo
    {
        public LlllvarParseInfo() : base(IsoType.LLLLVAR,
            0)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid LLLLVAR field {field} {pos}");
            if (pos + 4 > buf.Length) throw new Exception($"Insufficient data for LLLLVAR header, pos {pos}");
            var len = DecodeLength(buf,
                pos,
                4);
            if (len < 0) throw new Exception($"Invalid LLLLVAR length {len}, field {field} pos {pos}");
            if (len + pos + 4 > buf.Length) throw new Exception($"Insufficient data for LLLLVAR field {field}, pos {pos}");

            string v;
            try
            {
                v = len == 0 ? "" : Encoding.ASCII.GetString(buf,
                    pos + 4,
                    len);
            }
            catch (Exception) { throw new Exception($"Insufficient data for LLLLVAR header, field {field} pos {pos}"); }

            //This is new: if the String's length is different from the specified
            // length in the buffer, there are probably some extended characters.
            // So we create a String from the rest of the buffer, and then cut it to
            // the specified length.
            if (v.Length != len)
                v = Encoding.GetString(buf,
                    pos + 4,
                    buf.Length - pos - 4).Substring(0,
                    len);
            if (custom == null)
                return new IsoValue(IsoType,
                    v,
                    len);
            var dec = custom.DecodeField(v);
            return dec == null ? new IsoValue(IsoType,
                v,
                len) : new IsoValue(IsoType,
                dec,
                len,
                custom);
        }

        public override IsoValue ParseBinary(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid bin LLLLVAR field {field} pos {pos}");
            if (pos + 2 > buf.Length) throw new Exception($"Insufficient data for bin LLLLVAR header, field {field} pos {pos}");

            var len = ((buf[pos] & 0xf0) >> 4) * 1000 + (buf[pos] & 0x0f) * 100 + ((buf[pos + 1] & 0xf0) >> 4) * 10 + (buf[pos + 1] & 0x0f);
            if (len < 0)
                throw new Exception($"Invalid bin LLLLVAR length {len}, field {field} pos {pos}");

            if (len + pos + 2 > buf.Length)
                throw new Exception($"Insufficient data for bin LLLLVAR field {field}, pos {pos}");
            if (custom == null)
            {
                return new IsoValue(IsoType,
                    Encoding.GetString(buf,
                        pos + 2,
                        len));
            }
            var dec = custom.DecodeField(Encoding.GetString(buf,
                pos + 2,
                len));
            return dec == null ? new IsoValue(IsoType,
                Encoding.GetString(buf,
                    pos + 2,
                    len)) : new IsoValue(IsoType,
                dec,
                custom);
        }
    }
}