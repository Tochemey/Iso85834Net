using System;
using System.Text;
using ModIso8583.Parse;
using Xunit;

namespace ModIso8583.Test.Parse
{
    public class TestEncoding
    {
        [Fact(Skip = "It will fail due to unsigned byte nature of c#")]
        public void WindowsToUtf8()
        {
            string data = "05ácido";
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            byte[] buf = encoding.GetBytes(data);
            LlvarParseInfo parser = new LlvarParseInfo
            {
                Encoding = Encoding.UTF8
            };
            IsoValue  field = parser.Parse(1, buf, 0, null);
            Assert.Equal(field.Value, data.Substring(2));
            parser.Encoding = encoding;
            field = parser.Parse(1, buf, 0, null);
            Assert.Equal(data.Substring(2), field.Value);
        }
    }
}