using System;

namespace ModIso8583.Parse
{
    /// <summary>
    ///     This is the class used to parse ALPHA fields.
    /// </summary>
    public class AlphaParseInfo : AlphaNumericParseInfo
    {
        public AlphaParseInfo(int length) : base(IsoType.ALPHA,
            length)
        { }

        public override IsoValue ParseBinary(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid bin ALPHA field {field} position {pos}");
            if (pos + Length > buf.Length) throw new Exception($"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");
            try
            {
                if (custom == null)
                    return new IsoValue(IsoType,
                        Encoding.GetString(buf,
                            pos,
                            Length),
                        Length);
                var decoded = custom.DecodeField(Encoding.GetString(buf,
                    pos,
                    Length));
                return decoded == null ? new IsoValue(IsoType,
                    Encoding.GetString(buf,
                        pos,
                        Length),
                    Length) : new IsoValue(IsoType,
                    decoded,
                    Length,
                    custom);
            }
            catch (Exception) { throw new Exception($"Insufficient data for {IsoType} field {field} of length {Length}, pos {pos}"); }
        }
    }
}