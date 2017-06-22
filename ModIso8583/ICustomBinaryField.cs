namespace ModIso8583
{
    public interface ICustomBinaryField : ICustomField
    {
        /// <summary>
        /// Decode a custom binary field
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        object DecodeBinaryField(byte[] bytes,
            int offset,
            int length);

        /// <summary>
        /// Encode a custom binary field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        byte[] EncodeBinaryField(object value);
    }
}