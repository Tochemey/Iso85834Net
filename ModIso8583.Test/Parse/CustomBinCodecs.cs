using System;
using System.Numerics;
using ModIso8583.Codecs;
using Xunit;
using C5;
using ModIso8583.Parse;
using ModIso8583.Util;

namespace ModIso8583.Test.Parse
{
    public class CustomBinCodecs
    {
        private BigInteger b29 = BigInteger.Parse("12345678901234567890123456789");
        byte[] longData2 = new byte[] { 0x12, 0x34, 0x56, 0x78, (byte)0x90, 00, 00, 00, 00, 00 };
        byte[] bigintData1 = new byte[] { 1, 0x23, 0x45, 0x67, (byte)0x89, 1, 0x23, 0x45, 0x67, (byte)0x89, 1, 0x23, 0x45, 0x67, (byte)0x89, 00, 00, 00, 00, 00 };


        [Fact]
        public void TestLongCodec()
        {
            LongBcdCodec longCodec = new LongBcdCodec();
            byte[] data1 = new byte[] { 1, 0x23, 0x45, (byte)0x67, (byte)0x89, 00, 00, 00, 00, 00 };
            Assert.Equal(123456789L, (long)longCodec.DecodeBinaryField(data1, 0, 5));
            Assert.Equal(1234567890L, (long)longCodec.DecodeBinaryField(longData2, 0, 5));
            byte[] cod1 = longCodec.EncodeBinaryField(123456789L);
            byte[] cod2 = longCodec.EncodeBinaryField(1234567890L);
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal(data1[i], cod1[i]);
                Assert.Equal(longData2[i], cod2[i]);
            }
        }

        [Fact]
        public void TestBigIntCodec()
        {
            BigInteger b30 = BigInteger.Parse("123456789012345678901234567890");
            BigIntBcdCodec bigintCodec = new BigIntBcdCodec();
            byte[] data2 = new byte[] { 0x12, 0x34, 0x56, 0x78, (byte)0x90, 0x12, 0x34, 0x56, 0x78, (byte)0x90, 0x12, 0x34, 0x56, (byte)0x78, (byte)0x90, 00, 00, 00, 00, 00 };
            Assert.Equal(b29, bigintCodec.DecodeBinaryField(bigintData1, 0, 15));
            Assert.Equal(b30, bigintCodec.DecodeBinaryField(data2, 0, 15));
            byte[] cod1 = bigintCodec.EncodeBinaryField(b29);
            byte[] cod2 = bigintCodec.EncodeBinaryField(b30);
            for (int i = 0; i < 15; i++)
            {
                Assert.Equal(bigintData1[i], cod1[i]);
                Assert.Equal(data2[i], cod2[i]);
            }
        }

        private void TestFieldType(IsoType type, ModIso8583.Parse.FieldParseInfo fieldParser, int offset1, int offset2)
        {
            BigIntBcdCodec bigintCodec = new BigIntBcdCodec();
            LongBcdCodec longCodec = new LongBcdCodec();
            MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();
            IsoMessage tmpl = new IsoMessage()
            {
                Binary = true,
                Type = 0x200
            };
            tmpl.SetValue(2, 1234567890L, longCodec, type, 0);
            tmpl.SetValue(3, b29, bigintCodec, type, 0);
            mfact.AddMessageTemplate(tmpl);
            mfact.SetCustomField(2, longCodec);
            mfact.SetCustomField(3, bigintCodec);
            HashDictionary<int, FieldParseInfo> parser = new HashDictionary<int, FieldParseInfo>();
            parser.Add(2, fieldParser);
            parser.Add(3, fieldParser);
            mfact.SetParseMap(0x200, parser);
            mfact.UseBinary = true;

            //Test encoding
            tmpl = mfact.NewIsoMessage(0x200);
            byte[] buf = tmpl.WriteData();
            Console.WriteLine("MESSAGE: " + HexCodec.HexEncode(buf, 0, buf.Length));
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal(longData2[i], buf[i + offset1]);
            }
            for (int i = 0; i < 15; i++)
            {
                Assert.Equal(bigintData1[i], buf[i + offset2]);
            }
            //Test parsing
            tmpl = mfact.ParseMessage(buf, 0);
            Assert.Equal(1234567890L, tmpl.GetObjectValue(2));
            Assert.Equal(b29, tmpl.GetObjectValue(3));
        }

        [Fact]
        public void TestLLBIN(){
            TestFieldType(IsoType.LLBIN, new LlbinParseInfo(), 11, 17);
        }

        [Fact]
        public void TestLLLBIN(){
            TestFieldType(IsoType.LLLBIN, new LlbinParseInfo(), 12, 19);
        }
    }
}
