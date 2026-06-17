using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.IO;

namespace GribSharp.Tests
{
    [TestClass]
    public class Grib2ReaderTests
    {
        [TestMethod]
        public void ReadsBigEndianIntegers()
        {
            var data = new byte[] { 0x12, 0x34, 0x56, 0x78 };
            var r = new Grib2Reader(data);
            Assert.AreEqual((ushort)0x1234, r.ReadUInt16());
            Assert.AreEqual((ushort)0x5678, r.ReadUInt16());
        }

        [TestMethod]
        public void SignedMagnitudeNegative()
        {
            // 0x80000005 => sign bit set, magnitude 5 => -5
            var data = new byte[] { 0x80, 0x00, 0x00, 0x05 };
            var r = new Grib2Reader(data);
            Assert.AreEqual(-5, r.ReadInt32Sm());
        }

        [TestMethod]
        public void ReadsFloat32BigEndian()
        {
            // 1.0f big-endian = 0x3F800000
            var data = new byte[] { 0x3F, 0x80, 0x00, 0x00 };
            var r = new Grib2Reader(data);
            Assert.AreEqual(1.0f, r.ReadFloat32(), 1e-9f);
        }
    }
}
