using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.IO;
using GribSharp.Sections;

namespace GribSharp.Tests
{
    [TestClass]
    public class IdentificationSectionTests
    {
        [TestMethod]
        public void ReadsCenterAndReferenceTime()
        {
            // Construir sección 1 mínima de 21 bytes.
            var b = new byte[21];
            uint len = 21;
            b[0] = (byte)(len >> 24); b[1] = (byte)(len >> 16); b[2] = (byte)(len >> 8); b[3] = (byte)len;
            b[4] = 1;                 // número de sección
            b[5] = 0; b[6] = 7;       // centro = 7 (NCEP)
            // año en bytes 12-13 (offset dentro de sección): octetos 13-14 (1-based)
            b[12] = (byte)(2024 >> 8); b[13] = (byte)(2024 & 0xFF);
            b[14] = 6;  // mes
            b[15] = 17; // día
            b[16] = 0;  // hora
            b[17] = 0;  // min
            b[18] = 0;  // seg
            var r = new Grib2Reader(b);
            var hdr = SectionHeader.Read(r);
            Assert.AreEqual(1, hdr.Number);
            var s = IdentificationSection.Read(r, 0, hdr.Length);
            Assert.AreEqual(7, s.CenterId);
            Assert.AreEqual(new DateTime(2024, 6, 17, 0, 0, 0, DateTimeKind.Utc), s.ReferenceTime);
        }
    }
}
