using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Sections;
using GribSharp.DataRepresentation;

namespace GribSharp.Tests
{
    [TestClass]
    public class Jpeg2000Tests
    {
        [TestMethod]
        public void AppliesGribScalingToDecodedIntegers()
        {
            var drs = new DataRepresentationSection
            {
                Template = 40, ReferenceValue = 10f,
                BinaryScaleFactor = 1, DecimalScaleFactor = 1, BitsPerValue = 8
            };
            // Inyectar enteros decodificados [0, 5] => (10 + 0*2)/10=1.0 ; (10 + 5*2)/10=2.0
            var dec = new Jpeg2000Decoder(_ => new int[] { 0, 5 });
            var vals = dec.Decode(drs, new byte[] { 0x01 }, 2);
            Assert.AreEqual(1.0f, vals[0], 1e-5f);
            Assert.AreEqual(2.0f, vals[1], 1e-5f);
        }
    }
}
