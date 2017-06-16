using System;
using System.Text;

namespace ModIso8583.Parse
{
    public class LllvarParseInfo : FieldParseInfo
    {
        public LllvarParseInfo() : base(IsoType.LLLVAR,
            0)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid LLLVAR field {field} pos {pos}");
            if (pos + 3 > buf.Length) throw new Exception($"Insufficient data for LLLVAR header field {field} pos {pos}");
            var len = DecodeLength(buf,
                pos,
                3);
            if (len < 0) throw new Exception($"Invalid LLLVAR length {len}({Encoding.GetString(buf, pos, 3)}) field {field} pos {pos}");
            if (len + pos + 3 > buf.Length) throw new Exception($"Insufficient data for LLLVAR field {field}, pos {pos}");
            string v;
            try
            {
                v = len == 0 ? "" : Encoding.GetString(buf,
                    pos + 3,
                    len);
            }
            catch (Exception) { throw new Exception($"Insufficient data for LLLVAR header, field {field} pos {pos}"); }

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
            if (pos < 0) throw new Exception($"Invalid bin LLLVAR field {field} pos {pos}");
            if (pos + 2 > buf.Length) throw new Exception($"Insufficient data for bin LLLVAR header, field {field} pos {pos}");
            var len = (buf[pos] & 0x0f) * 100 + ((buf[pos + 1] & 0xf0) >> 4) * 10 + (buf[pos + 1] & 0x0f);
            if (len < 0) throw new Exception($"Invalid bin LLLVAR length {len}, field {field} pos {pos}");
            if (len + pos + 2 > buf.Length) throw new Exception($"Insufficient data for bin LLLVAR field {field}, pos {pos}");

            if (custom == null)
            {
                return new IsoValue(IsoType,
                    Encoding.GetString(buf,
                        pos + 2,
                        len));
            }
            var v = new IsoValue(IsoType,
                custom.DecodeField(Encoding.GetString(buf,
                    pos + 2,
                    len)),
                custom);
            if (v.Value == null)
                return new IsoValue(IsoType,
                    Encoding.ASCII.GetString(buf,
                        pos + 2,
                        len));
            return v;
        }
    }
}