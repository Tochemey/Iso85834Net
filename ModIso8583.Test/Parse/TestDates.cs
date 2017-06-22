using System;
using System.Text;
using ModIso8583.Parse;
using Xunit;
using System.IO;
using FluentAssertions;

namespace ModIso8583.Test.Parse
{
    public class TestDates
    {

        [Fact]
        public void TestDate4FutureTolerance()
        {
            DateTime today = DateTime.UtcNow;
            DateTime soon = today.AddMilliseconds(50000);
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
            MemoryStream stream = new MemoryStream();
            comp.Write(stream, true, false);
            IsoValue bin = new Date4ParseInfo().ParseBinary(0, stream.ToArray(), 0, null);
            DateTime dt = (DateTime)(comp.Value);
            DateTime bindt = (DateTime)bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate10FutureTolerance()
        {
            DateTime today = DateTime.UtcNow;
            DateTime soon = today.AddMilliseconds(50000);
            var buf = Encoding.UTF8.GetBytes(IsoType.DATE10.Format(soon));
            var comp = new Date10ParseInfo().Parse(0, buf, 0, null);
            DateTime v = (DateTime)comp.Value;
            Assert.True(v.CompareTo(DateTime.Now) > 0);
            MemoryStream stream = new MemoryStream();
            comp.Write(stream, true, false);
            IsoValue bin = new Date10ParseInfo().ParseBinary(0, stream.ToArray(), 0, null);
            DateTime dt = (DateTime)(comp.Value);
            DateTime bindt = (DateTime)bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate12FutureTolerance()
        {
            DateTime soon = DateTime.UtcNow.AddMilliseconds(50000);
            var buf = Encoding.UTF8.GetBytes(IsoType.DATE12.Format(soon));
            var comp = new Date12ParseInfo().Parse(0, buf, 0, null);
            DateTime v = (DateTime)comp.Value;
            Assert.True(v.CompareTo(DateTime.UtcNow) > 0);
            MemoryStream stream = new MemoryStream();
            comp.Write(stream, true, false);
            IsoValue bin = new Date12ParseInfo().ParseBinary(0, stream.ToArray(), 0, null);
            DateTime dt = (DateTime)(comp.Value);
            DateTime bindt = (DateTime)bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate14FutureTolerance()
        {
            DateTime soon = DateTime.UtcNow.AddMilliseconds(50000);
            var buf = Encoding.UTF8.GetBytes(IsoType.DATE14.Format(soon));
            IsoValue comp = new Date14ParseInfo().Parse(0, buf, 0, null);
            DateTime v = (DateTime)comp.Value;
            Assert.True(v.CompareTo(DateTime.UtcNow) > 0);
            MemoryStream stream = new MemoryStream();
            comp.Write(stream, true, false);
            IsoValue bin = new Date14ParseInfo().ParseBinary(0, stream.ToArray(), 0, null);
            DateTime dt = (DateTime)(comp.Value);
            DateTime bindt = (DateTime)bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }
    }
}