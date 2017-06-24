using System;
using System.Text;
using ModIso8583.Parse;
using ModIso8583.Util;
using Xunit;

namespace ModIso8583.Test.Parse
{
    public class TestEncoding
    {
        [Fact]
        public void WindowsToUtf8()
        {
            string data = "05ácido";
            Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
            sbyte[] buf = data.GetSbytes(encoding);
            LlvarParseInfo parser = new LlvarParseInfo
            {
                Encoding = Encoding.Default
            };
            IsoValue  field = parser.Parse(1, buf, 0, null);
            Assert.Equal(field.Value, data.Substring(2));
            parser.Encoding = encoding;
            field = parser.Parse(1, buf, 0, null);
            Assert.Equal(data.Substring(2), field.Value);
        }
    }
}