using System;
using Xunit;
using System.Text;
using System.IO;
using ModIso8583.Parse;

namespace ModIso8583.Test
{
    public class TestEbcdic
    {
        private IsoValue llvar = new IsoValue(IsoType.LLVAR, "Testing, testing, 123");

        [Fact]
        public void TestAscii()
        {
            llvar.Encoding = Encoding.UTF8;
            MemoryStream bout = new MemoryStream();
            llvar.Write(bout, false, false);
            byte[] buf = bout.ToArray();
            Assert.Equal(50, buf[0]);
            Assert.Equal(49, buf[1]);
            LlvarParseInfo parser = new LlvarParseInfo()
            {
                Encoding = Encoding.UTF8
            };

            IsoValue field = parser.Parse(1, buf, 0, null);
            Assert.Equal(llvar.Value, field.Value);
        }

        [Fact]
        public void TestEbcdic0()
        {
            llvar.Encoding = Encoding.GetEncoding(1047);
            MemoryStream bout = new MemoryStream();
            llvar.Write(bout, false, true);
            byte[] buf = bout.ToArray();
            Assert.Equal((byte)242, buf[0]);
            Assert.Equal((byte)241, buf[1]);
            LlvarParseInfo parser = new LlvarParseInfo()
            {
                Encoding = Encoding.GetEncoding(1047),
                ForceStringDecoding = true
            };
            IsoValue field = parser.Parse(1, buf, 0, null);
            Assert.Equal(llvar.Value, field.Value);
        }

        [Fact]
        public void TestParsers()
        {
            byte[] stringA = Encoding.GetEncoding(1047).GetBytes("A");
            LllvarParseInfo lllvar = new LllvarParseInfo()
            {
                Encoding = Encoding.GetEncoding(1047),
                ForceStringDecoding = true
            };
            IsoValue field = lllvar.Parse(1, new byte[] { (byte)240, (byte)240, (byte)241, (byte)193 }, 0, null);
            string string0 = Encoding.GetEncoding(1047).GetString(stringA);
            Assert.Equal(string0, field.Value);

            LllbinParseInfo lllbin = new LllbinParseInfo()
            {
                Encoding = Encoding.GetEncoding(1047),
                ForceStringDecoding = true
            };
            field = lllbin.Parse(1, new byte[] { (byte)240, (byte)240, (byte)242, 67, 49 }, 0, null);
            Assert.Equal(stringA, (byte[])field.Value);

            LlbinParseInfo llbin = new LlbinParseInfo()
            {
                Encoding = Encoding.GetEncoding(1047),
                ForceStringDecoding = true
            };
            field = llbin.Parse(1, new byte[] { (byte)240, (byte)242, 67, 49 }, 0, null);
            Assert.Equal(stringA, (byte[])field.Value);
        }

        [Fact]
        public void TestMessageType()
        {
            //todo to implement
        }
    }
}
