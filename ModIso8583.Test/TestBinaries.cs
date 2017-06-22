using System;
using System.Globalization;
using System.Text;
using ModIso8583.Util;
using Xunit;
using System.IO;

namespace ModIso8583.Test
{
    public class TestBinaries
    {
        public TestBinaries()
        {
            string configXml = @"/Resources/config.xml";
            mfactAscii.Encoding = Encoding.UTF8;
            mfactAscii.SetConfigPath(configXml);
            mfactAscii.AssignDate = true;

            mfactBin.Encoding = Encoding.UTF8;
            mfactBin.SetConfigPath(configXml);
            mfactBin.UseBinary = true;
            mfactBin.AssignDate = true;
        }

        private readonly MessageFactory<IsoMessage> mfactAscii = new MessageFactory<IsoMessage>();
        private readonly MessageFactory<IsoMessage> mfactBin = new MessageFactory<IsoMessage>();

        private void TestParsed(IsoMessage m)
        {
            Assert.Equal(m.Type,
                0x600);
            Assert.Equal(decimal.Parse("1234.00"),
                m.GetObjectValue(4));
            Assert.True(m.HasField(7),
                "No field 7!");
            Assert.Equal("000123",
                m.GetField(11).ToString()); // Wrong Trace
            var buf = (byte[]) m.GetObjectValue(41);
            byte[] exp =
            {
                0xab,
                0xcd,
                0xef,
                0,
                0,
                0,
                0,
                0
            };
            Assert.Equal(8,
                buf.Length); //Field 41 wrong length

            Assert.Equal(exp,
                buf); //"Field 41 wrong value"

            buf = (byte[]) m.GetObjectValue(42);
            exp = new byte[]
            {
                0x0a,
                0xbc,
                0xde,
                0
            };
            Assert.Equal(4,
                buf.Length); // "field 42 wrong length"
            Assert.Equal(exp,
                buf); // "Field 42 wrong value"
            Assert.True(((string) m.GetObjectValue(43)).StartsWith("Field of length 40",
                StringComparison.Ordinal));

            buf = (byte[]) m.GetObjectValue(62);
            exp = new byte[]
            {
                1,
                0x23,
                0x45,
                0x67,
                0x89,
                0xab,
                0xcd,
                0xef,
                0x62,
                1,
                0x23,
                0x45,
                0x67,
                0x89,
                0xab,
                0xcd
            };
            Assert.Equal(exp,
                buf);
            buf = (byte[]) m.GetObjectValue(64);
            exp[8] = 0x64;
            Assert.Equal(exp,
                buf);
            buf = (byte[]) m.GetObjectValue(63);
            exp = new byte[]
            {
                0,
                0x12,
                0x34,
                0x56,
                0x78,
                0x63
            };
            Assert.Equal(exp,
                buf);
            buf = (byte[]) m.GetObjectValue(65);
            exp[5] = 0x65;
            Assert.Equal(exp,
                buf);
        }

        [Fact]
        public void TestMessages()
        {
            //Create a message with both factories
            var ascii = mfactAscii.NewMessage(0x600);
            var bin = mfactBin.NewMessage(0x600);
            Assert.False(ascii.Binary || ascii.BinBitmap);
            Assert.True(bin.Binary);
            //HEXencode the binary message, headers should be similar to the ASCII version
            byte[] v = bin.WriteData();
            var hexBin = HexCodec.HexEncode(v, 0, v.Length);
            var hexAscii = Encoding.UTF8.GetString(ascii.WriteData()).ToUpper(CultureInfo.CurrentCulture);

            Assert.Equal("0600", hexBin.Substring(0, 4));

            //Should be the same up to the field 42 (first 80 chars)
            Assert.Equal(hexAscii.Substring(0, 88), hexBin.Substring(0, 88));
            Assert.Equal(ascii.GetObjectValue(43), Encoding.UTF8.GetString(v, 44, 40).Trim());
            //Parse both messages
            byte[] asciiBuf = ascii.WriteData();
            IsoMessage ascii2 = mfactAscii.ParseMessage(asciiBuf, 0);
            TestParsed(ascii2);
            Assert.Equal(ascii.GetObjectValue(7).ToString(), ascii2.GetObjectValue(7).ToString());
            IsoMessage bin2 = mfactBin.ParseMessage(bin.WriteData(), 0);
            //Compare values, should be the same
            TestParsed(bin2);
            Assert.Equal(bin.GetObjectValue(7).ToString(), bin2.GetObjectValue(7).ToString());

            //Test the debug string
            ascii.SetValue(60, "XXX", IsoType.LLVAR, 0);
            bin.SetValue(60, "XXX", IsoType.LLVAR, 0);
            Assert.Equal(ascii.DebugString(), bin.DebugString()); // "Debug strings differ"
            Assert.True(ascii.DebugString().Contains("03XXX"), "LLVAR fields wrong");
        }
    }
}