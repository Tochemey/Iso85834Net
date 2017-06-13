using System;
using System.Numerics;
using ModIso8583.Util;

namespace ModIso8583.Codecs
{
    /// <summary>
    ///     A custom field encoder/decoder to be used with LLBIN/LLLBIN fields
    ///     hat contain BigIntegers in BCD encoding.
    /// </summary>
    public class BigIntBcdCodec : ICustomBinaryField
    {
        public object DecodeField(string val)
        {
            return new BigInteger(Convert.ToInt32(val,
                10));
        }

        public string EncodeField(object obj) { return obj.ToString(); }

        public object DecodeBinaryField(byte[] bytes,
            int offset,
            int length)
        {
            return Bcd.DecodeToBigInteger(bytes,
                offset,
                length * 2);
        }

        public byte[] EncodeBinaryField(object val)
        {
            var value = (BigInteger) val;
            var s = value.ToString();
            var buf = new byte[s.Length / 2 + s.Length % 2];
            Bcd.Encode(s,
                buf);
            return buf;
        }
    }
}