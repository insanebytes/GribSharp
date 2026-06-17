using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Sections;
using GribSharp.DataRepresentation;

namespace GribSharp.Tests
{
    [TestClass]
    public class IeeeFloatTests
    {
        [TestMethod]
        public void DecodesRawFloats()
        {
            // 1.0f = 3F800000, 2.0f = 40000000 (big-endian)
            var data = new byte[] { 0x3F, 0x80, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00 };
            var drs = new DataRepresentationSection { Template = 4 };
            var dec = new IeeeFloatDecoder();
            var vals = dec.Decode(drs, data, 2);
            Assert.AreEqual(1.0f, vals[0], 1e-9f);
            Assert.AreEqual(2.0f, vals[1], 1e-9f);
        }
    }
}
