using System;
using System.Text;
using ModIso8583.Parse;
using Xunit;

namespace ModIso8583.Test.Parse
{
    public class TestDates
    {

        [Fact]
        public void TestDate4FutureTolerance()
        {
            DateTime today = DateTime.UtcNow;
            DateTime soon =  today.AddMilliseconds(50000);
            today = today.AddHours(0)
            .AddMinutes(0)
            .AddSeconds(0)
            .AddMilliseconds(0);
            var buf = Encoding.UTF8.GetBytes(IsoType.DATE4.Format(soon));
            var comp = new Date4ParseInfo().Parse(0,
                buf,
                0,
                null);
            Assert.Equal(comp.Value,
                today.Date);
        }
    }
}