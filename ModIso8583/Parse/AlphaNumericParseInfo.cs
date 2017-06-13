using System;

namespace ModIso8583.Parse
{
    /// <summary>
    ///     This is the common abstract superclass to parse ALPHA and NUMERIC field types.
    /// </summary>
    public abstract class AlphaNumericParseInfo : FieldParseInfo
    {
        public AlphaNumericParseInfo(IsoType isoType,
            int length) : base(isoType,
            length)
        { }

        public override IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new Exception($"Invalid ALPHA/NUM field {field} position {pos}");
            if (pos + Length > buf.Length) throw new Exception($"Insufficient data for {IsoType} field {field} of length {Length}, pos {pos}");
            try
            {
                var v = Encoding.GetString(buf,
                    pos,
                    Length);
                if (v.Length != Length)
                    v = Encoding.GetString(buf,
                        pos,
                        buf.Length - pos).Substring(0,
                        Length);
                if (custom == null)
                    return new IsoValue(IsoType,
                        v,
                        Length);
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