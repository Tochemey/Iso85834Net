using System;
using ModIso8583.Util;

namespace ModIso8583.Parse
{
    /// <summary>
    ///     This is the class used to parse ALPHA fields.
    /// </summary>
    public class AlphaParseInfo : AlphaNumericFieldParseInfo
    {
        public AlphaParseInfo(int length) : base(IsoType.ALPHA,
            length)
        { }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid bin ALPHA field {field} position {pos}");
            if (pos + Length > buf.Length) throw new Exception($"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");
            try
            {
                string v;
                if (custom == null)
                {
                    v = buf.SbyteString(pos,
                        Length,
                        Encoding);
                    return new IsoValue(IsoType,
                        v,
                        Length);
                }

                v = buf.SbyteString(pos,
                    Length,
                    Encoding);

                var decoded = custom.DecodeField(v);
                return decoded == null ? new IsoValue(IsoType,
                    v,
                    Length) : new IsoValue(IsoType,
                    decoded,
                    Length,
                    custom);
            }
            catch (Exception) { throw new Exception($"Insufficient data for {IsoType} field {field} of length {Length}, pos {pos}"); }
        }
    }
}