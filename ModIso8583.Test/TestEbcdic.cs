using System;
using Xunit;
using System.Text;
using System.IO;
using ModIso8583.Parse;
using C5;

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
            IsoMessage msg = new IsoMessage();
            msg.Type = 0x1100;
            msg.Encoding = Encoding.GetEncoding(1047);
            msg.BinBitmap = true;
            byte[] enc = msg.WriteData();
            Assert.Equal(12, enc.Length);
            Assert.Equal((byte)241, enc[0]);
            Assert.Equal((byte)241, enc[1]);
            Assert.Equal((byte)240, enc[2]);
            Assert.Equal((byte)240, enc[3]);
            MessageFactory<IsoMessage> mf = new MessageFactory<IsoMessage>();
            HashDictionary<int, FieldParseInfo> pmap = new HashDictionary<int, FieldParseInfo>();
            mf.ForceStringEncoding = true;
            mf.UseBinaryBitmap = true;
            mf.Encoding = Encoding.GetEncoding(1047);
            mf.SetParseMap(0x1100, pmap);
            IsoMessage m2 = mf.ParseMessage(enc, 0);
            Assert.Equal(msg.Type, m2.Type);

            //Now with text bitmap
            msg.BinBitmap = false;
            msg.ForceStringEncoding = true;
            byte[] enc2 = msg.WriteData();
            Assert.Equal(20, enc2.Length);
            mf.UseBinaryBitmap = false;
            m2 = mf.ParseMessage(enc2, 0);
            Assert.Equal(msg.Type, m2.Type);
        }

        [Fact]
        public void TestDate4()
        {
            Date4ParseInfo parser = new Date4ParseInfo()
            {
                Encoding = Encoding.GetEncoding(1047),
                ForceStringDecoding = true
            };
            IsoValue v = parser.Parse(1, new byte[] { (byte)240, (byte)241, (byte)242, (byte)245 }, 0, null);
            DateTime val = (DateTime)v.Value;
            Assert.Equal("01", val.ToString("MM"));
            Assert.Equal("25", val.ToString("dd"));
        }

        [Fact]
        public void TestDate10()
        {
            Date10ParseInfo parser = new Date10ParseInfo()
            {
                Encoding = Encoding.GetEncoding(1047),
                ForceStringDecoding = true
            };
            IsoValue v = parser.Parse(1, new byte[] { (byte)240, (byte)241, (byte)242, (byte)245, (byte)242, (byte)243, (byte)245, (byte)249, (byte)245, (byte)249 }, 0, null);
            DateTime val = (DateTime)v.Value;
            Assert.Equal("01", val.ToString("MM"));
            Assert.Equal("25", val.ToString("dd"));
            Assert.Equal("23:59:59", val.ToString("HH:mm:ss"));
        }

        [Fact]
        public void TestDateExp()
        {
            DateExpParseInfo parser = new DateExpParseInfo()
            {
                Encoding = Encoding.GetEncoding(1047),
                ForceStringDecoding = true
            };

            IsoValue v = parser.Parse(1, new byte[] { (byte)241, (byte)247, (byte)241, (byte)242 }, 0, null);
            DateTime val = (DateTime)v.Value;
            Assert.Equal("12", val.ToString("MM"));
            Assert.Equal("2017", val.ToString("yyyy"));
        }
    }
}
