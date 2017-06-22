using System;
using System.Text;
using ModIso8583.Codecs;
using ModIso8583.Util;
using Xunit;

namespace ModIso8583.Test
{
    public class TestConfigParser
    {
        private MessageFactory<IsoMessage> Config(string path)
        {
            MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();
            mfact.SetConfigPath(path);
            return mfact;
        }

        [Fact]
        public void TestParser()
        {
            string configXml = @"\Resources\config.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);

            //Headers
            Assert.NotNull(mfact.GetIsoHeader(0x800));
            Assert.NotNull(mfact.GetIsoHeader(0x810));
            Assert.Equal(mfact.GetIsoHeader(0x800), mfact.GetIsoHeader(0x810));

            //Templates
            IsoMessage m200 = mfact.GetMessageTemplate(0x200);
            Assert.NotNull(m200);
            IsoMessage m400 = mfact.GetMessageTemplate(0x400);
            Assert.NotNull(m400);

            for (int i = 2; i < 89; i++)
            {
                IsoValue  v = m200.GetField(i);
                if (v == null)
                {
                    Assert.False(m400.HasField(i));
                }
                else
                {
                    Assert.True(m400.HasField(i));
                    Assert.Equal(v, m400.GetField(i));
                }
            }

            Assert.False(m200.HasField(90));
            Assert.True(m400.HasField(90));
            Assert.True(m200.HasField(102));
            Assert.False(m400.HasField(102));

            //Parsing guides
            string s800 = "0800201080000000000012345611251125";
            string s810 = "08102010000002000000123456112500";
            IsoMessage m = mfact.ParseMessage(Encoding.UTF8.GetBytes(s800), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.True(m.HasField(17));
            Assert.False(m.HasField(39));
            m = mfact.ParseMessage(Encoding.UTF8.GetBytes(s810),
                0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.False(m.HasField(17));
            Assert.True(m.HasField(39));
        }

        [Fact]
        public void TestSimpleCompositeParsers()
        {
            string configXml = @"\Resources\composites.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage m = mfact.ParseMessage("01000040000000000000016one  03two12345.".GetBytes(), 0);
            Assert.NotNull(m);
            CompositeField f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("one  ", f.GetObjectValue(0));
            Assert.Equal("two", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal(".", f.GetObjectValue(3));

            m = mfact.ParseMessage("01000040000000000000018ALPHA05LLVAR12345X".GetBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(10));
            f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f.GetField(0));
            Assert.NotNull(f.GetField(1));
            Assert.NotNull(f.GetField(2));
            Assert.NotNull(f.GetField(3));
            Assert.Null(f.GetField(4));
            Assert.Equal("ALPHA", f.GetObjectValue(0));
            Assert.Equal("LLVAR", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal("X", f.GetObjectValue(3));
        }

        [Fact]
        public void TestNestedCompositeParser()
        {
            string configXml = @"\Resources\composites.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage m = mfact.ParseMessage("01010040000000000000019ALPHA11F1F205F03F4X".GetBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(10));
            CompositeField f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f.GetField(0));
            Assert.NotNull(f.GetField(1));
            Assert.NotNull(f.GetField(2));
            Assert.Null(f.GetField(3));
            Assert.Equal("ALPHA", f.GetObjectValue(0));
            Assert.Equal("X", f.GetObjectValue(2));
            f = (CompositeField) f.GetObjectValue(1);
            Assert.Equal("F1", f.GetObjectValue(0));
            Assert.Equal("F2", f.GetObjectValue(1));
            f = (CompositeField) f.GetObjectValue(2);
            Assert.Equal("F03", f.GetObjectValue(0));
            Assert.Equal("F4", f.GetObjectValue(1));
        }

        [Fact]
        public void TestSimpleCompositeTemplate()
        {
            string configXml = @"\Resources\composites.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage m = mfact.NewMessage(0x100);
            //Simple composite
            Assert.NotNull(m);
            Assert.False(m.HasField(1));
            Assert.False(m.HasField(2));
            Assert.False(m.HasField(3));
            Assert.False(m.HasField(4));
            CompositeField f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal(f.GetObjectValue(0), "abcde");
            Assert.Equal(f.GetObjectValue(1), "llvar");
            Assert.Equal(f.GetObjectValue(2), "12345");
            Assert.Equal(f.GetObjectValue(3), "X");
            Assert.False(m.HasField(4));
        }
    }
}