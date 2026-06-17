using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Sections;
using GribSharp.DataRepresentation;

namespace GribSharp.Tests
{
    [TestClass]
    public class ComplexPackingTests
    {
        // Construye RawTemplateBytes (octetos 22..) para 1 grupo con anchura fija.
        private static byte[] RawTemplate(
            int ng, int refGroupWidth, int bitsGroupWidth,
            int refGroupLength, int lengthIncrement, int lastGroupLength,
            int bitsGroupLength, int order = -1, int extraBytes = 0)
        {
            // 5.2 si order<0 (26 bytes: octetos 22-47), 5.3 si order>=0 (+octetos 48-49)
            int size = order < 0 ? 26 : 28;
            var b = new byte[size];
            b[0] = 1;  // 22 splitting
            b[1] = 0;  // 23 missing mgmt = none
            // 24-27, 28-31 sustitutos (ceros)
            b[10] = (byte)(ng >> 24); b[11] = (byte)(ng >> 16); b[12] = (byte)(ng >> 8); b[13] = (byte)ng; // 32-35 NG
            b[14] = (byte)refGroupWidth;   // 36
            b[15] = (byte)bitsGroupWidth;  // 37
            b[16] = (byte)(refGroupLength >> 24); b[17] = (byte)(refGroupLength >> 16);
            b[18] = (byte)(refGroupLength >> 8); b[19] = (byte)refGroupLength; // 38-41
            b[20] = (byte)lengthIncrement; // 42
            b[21] = (byte)(lastGroupLength >> 24); b[22] = (byte)(lastGroupLength >> 16);
            b[23] = (byte)(lastGroupLength >> 8); b[24] = (byte)lastGroupLength; // 43-46
            b[25] = (byte)bitsGroupLength; // 47
            if (order >= 0) { b[26] = (byte)order; b[27] = (byte)extraBytes; } // 48-49
            return b;
        }

        [TestMethod]
        public void DecodesSingleGroupNoDifferencing()
        {
            // 1 grupo, 3 valores, anchura 8 bits, referencia de grupo 0, escala R=0,E=0,D=0.
            // Estructura de datos sección 7: [group references][group widths][group lengths][values]
            // group references: 1 grupo * refValueBits(=bitsPerValue=8) => 1 byte = 0
            // group widths: NG * bitsGroupWidth(=0) => 0 bits (anchura = refGroupWidth = 8)
            // group lengths: NG * bitsGroupLength(=0) => 0 bits (longitud = refGroupLength = 3)
            // values: 3 * 8 bits = 10,20,30
            var drs = new DataRepresentationSection
            {
                Template = 2, ReferenceValue = 0f,
                BinaryScaleFactor = 0, DecimalScaleFactor = 0, BitsPerValue = 8,
                RawTemplateBytes = RawTemplate(
                    ng: 1, refGroupWidth: 8, bitsGroupWidth: 0,
                    refGroupLength: 3, lengthIncrement: 1, lastGroupLength: 3,
                    bitsGroupLength: 0, order: -1)
            };
            var data = new byte[] { 0x00 /*group ref*/, 10, 20, 30 };
            var dec = new ComplexPackingDecoder();
            var vals = dec.Decode(drs, data, 3);
            CollectionAssert.AreEqual(new[] { 10f, 20f, 30f }, vals);
        }
    }
}
