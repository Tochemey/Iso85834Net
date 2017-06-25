using System;
using ModIso8583.Parse;
using Xunit;

namespace ModIso8583.Test
{
    public class TestIssue4
    {
        [Fact]
        public void TestTextBitmap()
        {
            MessageFactory<IsoMessage> tmf = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(tmf, @"/Resources/issue4.xml");
            IsoMessage tm = tmf.NewMessage(0x800);
            var bb = tm.WriteToBuffer(2);
            Assert.Equal(
                70, bb.Length); //"Wrong message length for new TXT"

            //todo to continue

            //Assert.Equal(68, BitConverter.ToInt16(bb, 0));

            //MessageFactory<IsoMessage> tmfp = new MessageFactory<IsoMessage>();
            //ConfigParser.configureFromClasspathConfig(tmfp, "issue4.xml");
            //byte[] buf2 = new byte[bb.remaining()];
            //bb.get(buf2);
            //tm = tmfp.parseMessage(buf2, 0);
            //final ByteBuffer bbp = tm.writeToBuffer(2);
            //Assert.assertArrayEquals("Parsed-reencoded TXT differs from original",
            //    bb.array(), bbp.array());
        }
    }
}