using System;
using Xunit;
using System.Globalization;
using System.Text;
namespace ModIso8583.Test.Parse
{
    public class TestDates
    {
        [Fact]
        public void TestDate4FutureTolerance()
        {
            DateTime today = DateTime.UtcNow;
            DateTime soon = new DateTime(today.Millisecond + 5000);
            today.AddHours(0);
            today.AddMinutes(0);
            today.AddSeconds(0);
            today.AddMilliseconds(0);
            byte[] buf = Encoding.ASCII.GetBytes(IsoType.DATE4.Format(soon));
            IsoValue comp = new ModIso8583.Parse.Date4ParseInfo().Parse(0, buf, 0, null);
            Assert.Equal(comp.Value, today.Date);
        }
    }
}
