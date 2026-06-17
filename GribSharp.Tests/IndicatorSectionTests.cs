using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.IO;
using GribSharp.Sections;
using GribSharp.Exceptions;

namespace GribSharp.Tests
{
    [TestClass]
    public class IndicatorSectionTests
    {
        private static byte[] Indicator(long length, byte discipline = 0, byte edition = 2)
        {
            var b = new byte[16];
            b[0] = (byte)'G'; b[1] = (byte)'R'; b[2] = (byte)'I'; b[3] = (byte)'B';
            b[6] = discipline;
            b[7] = edition;
            // bytes 8..15 = length (UInt64 big-endian)
            for (int i = 0; i < 8; i++)
                b[15 - i] = (byte)((length >> (8 * i)) & 0xFF);
            return b;
        }

        [TestMethod]
        public void ReadsDisciplineEditionLength()
        {
            var r = new Grib2Reader(Indicator(123456, discipline: 0, edition: 2));
            var s = IndicatorSection.Read(r);
            Assert.AreEqual(0, s.Discipline);
            Assert.AreEqual(2, s.Edition);
            Assert.AreEqual(123456L, s.TotalLength);
            Assert.AreEqual(16, r.Position);
        }

        [TestMethod]
        [ExpectedException(typeof(GribFormatException))]
        public void RejectsBadMagic()
        {
            var bad = new byte[16];
            IndicatorSection.Read(new Grib2Reader(bad));
        }
    }
}
