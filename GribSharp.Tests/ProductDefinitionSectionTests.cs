using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.IO;
using GribSharp.Sections;

namespace GribSharp.Tests
{
    [TestClass]
    public class ProductDefinitionSectionTests
    {
        [TestMethod]
        public void ParsesTemplate0_CategoryNumberLevelForecast()
        {
            // Sección 4, plantilla 4.0
            var b = new byte[40];
            uint len = (uint)b.Length;
            b[0] = (byte)(len >> 24); b[1] = (byte)(len >> 16); b[2] = (byte)(len >> 8); b[3] = (byte)len;
            b[4] = 4;            // sección
            b[5] = 0; b[6] = 0;  // NV (coordenadas)
            b[7] = 0; b[8] = 0;  // plantilla 4.0
            b[9] = 0;            // categoría (octeto 10) = 0 (temperatura)
            b[10] = 0;           // número (octeto 11) = 0 (TMP)
            b[11] = 2;           // tipo de proceso generador
            b[12] = 0;           // id proceso bg
            b[13] = 96;          // id proceso analisis/forecast
            // octetos 19-22 (offset 18): forecast time
            b[17] = 1;           // unidad de tiempo (h)
            b[18] = 0; b[19] = 0; b[20] = 0; b[21] = 6; // forecast time = 6
            b[22] = 103;         // tipo primera superficie (octeto 23) = 103 (alt sobre suelo)
            b[23] = 0;           // factor escala
            b[24] = 0; b[25] = 0; b[26] = 0; b[27] = 2; // valor escalado = 2 (m)
            var r = new Grib2Reader(b);
            var hdr = SectionHeader.Read(r);
            var pds = ProductDefinitionSection.Read(r, 0, hdr.Length);
            Assert.AreEqual(0, pds.Template);
            Assert.AreEqual(0, pds.ParameterCategory);
            Assert.AreEqual(0, pds.ParameterNumber);
            Assert.AreEqual(6, pds.ForecastTime);
            Assert.AreEqual(103, pds.LevelType);
            Assert.AreEqual(2.0, pds.LevelValue, 1e-9);
        }
    }
}
