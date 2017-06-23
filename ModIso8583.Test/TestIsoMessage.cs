using System;
using System.IO;
using System.Text;
using Xunit;

namespace ModIso8583.Test
{
    public class TestIsoMessage
    {
        public TestIsoMessage()
        {
            mf = new MessageFactory<IsoMessage>();
            mf.Encoding = Encoding.UTF8;
            mf.SetCustomField(48,
                new CustomField48());
            mf.SetConfigPath(@"/Resources/config.xml");
        }

        private readonly MessageFactory<IsoMessage> mf;

        [Fact]
        public void TestCreation()
        {
            var iso = mf.NewMessage(0x200);
            Assert.Equal(0x200,
                iso.Type);
            Assert.True(iso.HasEveryField(3,
                32,
                35,
                43,
                48,
                49,
                60,
                61,
                100,
                102));
            Assert.Equal(IsoType.NUMERIC,
                iso.GetField(3).Type);
            Assert.Equal("650000",
                iso.GetObjectValue(3));
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(32).Type);
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(35).Type);
            Assert.Equal(IsoType.ALPHA,
                iso.GetField(43).Type);
            Assert.Equal(40,
                ((string) iso.GetObjectValue(43)).Length);
            Assert.Equal(IsoType.LLLVAR,
                iso.GetField(48).Type);
            Assert.True(iso.GetObjectValue(48) is CustomField48);
            Assert.Equal(IsoType.ALPHA,
                iso.GetField(49).Type);
            Assert.Equal(IsoType.LLLVAR,
                iso.GetField(60).Type);
            Assert.Equal(IsoType.LLLVAR,
                iso.GetField(61).Type);
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(100).Type);
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(102).Type);

            for (var i = 4; i < 32; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 36; i < 43; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 50; i < 60; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 62; i < 100; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 103; i < 128; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
        }

        [Fact]
        public void TestEncoding()
        {
            var m1 = mf.NewMessage(0x200);
            var buf = m1.WriteData();
            var m2 = mf.ParseMessage(buf,
                mf.GetIsoHeader(0x200).Length);
            Assert.Equal(m2.Type,
                m1.Type);
            for (var i = 2; i < 128; i++)
                //Either both have the field or neither have it
                if (m1.HasField(i) && m2.HasField(i))
                {
                    Assert.Equal(m1.GetField(i).Type,
                        m2.GetField(i).Type);
                    Assert.Equal(m1.GetObjectValue(i),
                        m2.GetObjectValue(i));
                }
                else
                {
                    Assert.False(m1.HasField(i));
                    Assert.False(m2.HasField(i));
                }
        }

        [Fact]
        public void TestParsing()
        {
            byte[] buf = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"/Resources/parse1.txt");
            IsoMessage iso = mf.ParseMessage(buf, mf.GetIsoHeader(0x210).Length);
            Assert.Equal(0x210, iso.Type);
        }
    }
}