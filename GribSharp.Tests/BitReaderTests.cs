using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.IO;

namespace GribSharp.Tests
{
    [TestClass]
    public class BitReaderTests
    {
        [TestMethod]
        public void ReadsMsbFirstAcrossBytes()
        {
            // 0b1010_0110 0b1100_0000
            var data = new byte[] { 0xA6, 0xC0 };
            var br = new BitReader(data);
            Assert.AreEqual(0b101L, br.ReadBits(3)); // 5
            Assert.AreEqual(0b00110L, br.ReadBits(5)); // 6
            Assert.AreEqual(0b11L, br.ReadBits(2)); // 3
        }

        [TestMethod]
        public void ZeroBitsReturnsZero()
        {
            var br = new BitReader(new byte[] { 0xFF });
            Assert.AreEqual(0L, br.ReadBits(0));
        }
    }
}
