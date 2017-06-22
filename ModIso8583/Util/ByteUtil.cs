using System;

namespace ModIso8583.Util
{
    public static class ByteUtil
    {
        /// <summary>
        /// Convert all unsigned bytes to signed bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static sbyte[] ToSbytes(this byte[] bytes) => Array.ConvertAll(bytes, b => unchecked((sbyte)b));
    }
}