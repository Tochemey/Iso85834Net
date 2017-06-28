using System;
using System.Text;
using Iso85834Net;
using Iso85834Net.Parse;
using Iso85834Net.Util;
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
            if (OsUtil.IsLinux())
                encoding = Encoding.UTF8;
            sbyte[] buf = data.GetSbytes(encoding);
            LlvarParseInfo parser = new LlvarParseInfo
            {
                Encoding = Encoding.Default
            };

            if (OsUtil.IsLinux())
                parser.Encoding = Encoding.UTF8;

            IsoValue  field = parser.Parse(1, buf, 0, null);
            Assert.Equal(field.Value, data.Substring(2));
            parser.Encoding = encoding;
            field = parser.Parse(1, buf, 0, null);
            Assert.Equal(data.Substring(2), field.Value);
        }
    }
}