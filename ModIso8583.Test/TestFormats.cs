using System;
using TimeZoneConverter;
using Xunit;

namespace ModIso8583.Test
{
    public class TestFormats
    {
        private DateTimeOffset date = DateTimeOffset.FromUnixTimeMilliseconds(96867296000L);

        [Fact]
        public void TestDateFormats()
        {
            // UTC-06:00 in honor to Enrique Zamudio
            //var tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            var tz = TZConvert.GetTimeZoneInfo("Central Standard Time");
            date = TimeZoneInfo.ConvertTime(date,
                tz);
            
            Assert.Equal("0125213456", IsoType.DATE10.Format(date));
            Assert.Equal("0125", IsoType.DATE4.Format(date));
            Assert.Equal("7301", IsoType.DATE_EXP.Format(date));
            Assert.Equal("213456", IsoType.TIME.Format(date));
            Assert.Equal("730125213456", IsoType.DATE12.Format(date));
            Assert.Equal("19730125213456", IsoType.DATE14.Format(date));

            // Now UTC
            date = TimeZoneInfo.ConvertTime(date,
                TimeZoneInfo.Utc);
            Assert.Equal("0126033456", IsoType.DATE10.Format(date));
            Assert.Equal("0126", IsoType.DATE4.Format(date));
            Assert.Equal("7301", IsoType.DATE_EXP.Format(date));
            Assert.Equal("033456", IsoType.TIME.Format(date));
            Assert.Equal("730126033456", IsoType.DATE12.Format(date));
            Assert.Equal("19730126033456", IsoType.DATE14.Format(date));

            //Now with GMT+1
            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo("W. Europe Standard Time");
            date = TimeZoneInfo.ConvertTime(date,
                timeZoneInfo);

            Assert.Equal("0126043456", IsoType.DATE10.Format(date));
            Assert.Equal("0126", IsoType.DATE4.Format(date));
            Assert.Equal("7301", IsoType.DATE_EXP.Format(date));
            Assert.Equal("043456", IsoType.TIME.Format(date));
            Assert.Equal("730126043456", IsoType.DATE12.Format(date));
            Assert.Equal("19730126043456", IsoType.DATE14.Format(date));
        }

        [Fact]
        public void TestNumericFormats()
        {
            Assert.True(IsoType.NUMERIC.Format(123, 6).Equals("000123"));
            Assert.True(IsoType.NUMERIC.Format("hola", 6).Equals("00hola"));
            Assert.True(IsoType.AMOUNT.Format(12345, 0).Equals("000001234500"));
            Assert.True(IsoType.AMOUNT.Format(decimal.Parse("12345.67"), 0).Equals("000001234567"));
            Assert.True(IsoType.AMOUNT.Format("1234.56", 0).Equals("000000123456"));
        }

        [Fact]
        public void TestStringFormats()
        {
            Assert.True(IsoType.ALPHA.Format("hola", 3).Equals("hol"));
            Assert.True(IsoType.ALPHA.Format("hola", 4).Equals("hola"));
            Assert.True(IsoType.ALPHA.Format("hola", 6).Equals("hola  "));
            Assert.True(IsoType.LLVAR.Format("hola", 0).Equals("hola"));
            Assert.True(IsoType.LLLVAR.Format("hola", 0).Equals("hola"));
            Assert.True(IsoType.LLLLVAR.Format("HOLA", 0).Equals("HOLA"));
        }
    }
}