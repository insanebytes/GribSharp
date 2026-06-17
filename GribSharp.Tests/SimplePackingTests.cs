using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Sections;
using GribSharp.DataRepresentation;

namespace GribSharp.Tests
{
    [TestClass]
    public class SimplePackingTests
    {
        [TestMethod]
        public void DecodesValues()
        {
            // R=0, E=0, D=0, bits=8 => valor = X. Bytes de datos: 10, 20, 30
            var drs = new DataRepresentationSection
            {
                Template = 0,
                ReferenceValue = 0f,
                BinaryScaleFactor = 0,
                DecimalScaleFactor = 0,
                BitsPerValue = 8
            };
            var data = new byte[] { 10, 20, 30 };
            var dec = new SimplePackingDecoder();
            var vals = dec.Decode(drs, data, 3);
            CollectionAssert.AreEqual(new[] { 10f, 20f, 30f }, vals);
        }

        [TestMethod]
        public void AppliesScaling()
        {
            // R=5, E=1 (=>*2), D=1 (=>/10), bits=4. X=3 => (5 + 3*2)/10 = 1.1
            var drs = new DataRepresentationSection
            {
                Template = 0, ReferenceValue = 5f,
                BinaryScaleFactor = 1, DecimalScaleFactor = 1, BitsPerValue = 4
            };
            var data = new byte[] { 0x30 }; // 0011 0000 => primer nibble = 3
            var dec = new SimplePackingDecoder();
            var vals = dec.Decode(drs, data, 1);
            Assert.AreEqual(1.1f, vals[0], 1e-5f);
        }
    }
}
