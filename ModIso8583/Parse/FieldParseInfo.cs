using System.Text;

namespace ModIso8583.Parse
{
    /// <summary>
    ///     This class stores the necessary information for parsing an ISO8583 field
    ///     inside a message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TR"></typeparam>
    public abstract class FieldParseInfo
    {
        public FieldParseInfo(IsoType isoType,
            int length)
        {
            IsoType = isoType;
            Length = length;
        }

        public IsoType IsoType { get; }
        public int Length { get; }
        public bool ForceStringDecoding { get; }
        public ICustomField Decoder { get; set; }
        public Encoding Encoding { get; set; }

        /// <summary>
        ///     Parses the character data from the buffer and returns the IsoValue with the correct data type in it.
        /// </summary>
        /// <param name="field">he field index, useful for error reporting.</param>
        /// <param name="buf">The full ISO message buffer.</param>
        /// <param name="pos">The starting position for the field data.</param>
        /// <param name="custom">A CustomField to decode the field.</param>
        /// <returns></returns>
        public abstract IsoValue Parse(int field,
            byte[] buf,
            int pos,
            ICustomField custom);

        /// <summary>
        ///     Parses binary data from the buffer, creating and returning an IsoValue of the configured
        ///     type and length.
        /// </summary>
        /// <param name="field">he field index, useful for error reporting.</param>
        /// <param name="buf">The full ISO message buffer.</param>
        /// <param name="pos">The starting position for the field data.</param>
        /// <param name="custom">A CustomField to decode the field.</param>
        /// <returns></returns>
        public abstract IsoValue ParseBinary(int field,
            byte[] buf,
            int pos,
            ICustomField custom);

        /// <summary>
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="pos"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        protected int DecodeLength(byte[] buf,
            int pos,
            int digits)
        {
            if (ForceStringDecoding)
            {
                var string0 = Encoding.ASCII.GetString(buf,
                    pos,
                    digits);
                return int.Parse(string0);
            }
            switch (digits)
            {
                case 2: return (buf[pos] - 48) * 10 + (buf[pos + 1] - 48);
                case 3: return (buf[pos] - 48) * 100 + (buf[pos + 1] - 48) * 10 + (buf[pos + 2] - 48);
                case 4: return (buf[pos] - 48) * 1000 + (buf[pos + 1] - 48) * 100 + (buf[pos + 2] - 48) * 10 + (buf[pos + 3] - 48);
            }
            return -1;
        }
    }
}