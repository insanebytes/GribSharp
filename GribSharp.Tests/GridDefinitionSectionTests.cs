using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.IO;
using GribSharp.Sections;
using GribSharp.Exceptions;

namespace GribSharp.Tests
{
    [TestClass]
    public class GridDefinitionSectionTests
    {
        [TestMethod]
        public void ParsesLatLonTemplate0()
        {
            var b = BuildGds(ni: 1440, nj: 721,
                la1: 90_000000, lo1: 0, la2: -90_000000, lo2: 359_750000,
                di: 250000, dj: 250000, scan: 0);
            var r = new Grib2Reader(b);
            var hdr = SectionHeader.Read(r);
            var gds = GridDefinitionSection.Read(r, 0, hdr.Length);
            Assert.AreEqual(1440, gds.Grid.Ni);
            Assert.AreEqual(721, gds.Grid.Nj);
            Assert.AreEqual(90.0, gds.Grid.Lat1, 1e-6);
            Assert.AreEqual(359.75, gds.Grid.Lon2, 1e-6);
            Assert.AreEqual(0.25, gds.Grid.Di, 1e-9);
        }

        [TestMethod]
        [ExpectedException(typeof(GribNotSupportedException))]
        public void RejectsNonLatLonTemplate()
        {
            var b = BuildGds(1, 1, 0, 0, 0, 0, 0, 0, scan: 0, template: 30);
            var r = new Grib2Reader(b);
            SectionHeader.Read(r);
            GridDefinitionSection.Read(r, 0, (uint)b.Length);
        }

        private static void PutU32(byte[] b, int o, uint v)
        {
            b[o] = (byte)(v >> 24); b[o + 1] = (byte)(v >> 16); b[o + 2] = (byte)(v >> 8); b[o + 3] = (byte)v;
        }
        private static void PutI32Sm(byte[] b, int o, int v)
        {
            uint raw = (uint)(v < 0 ? (0x80000000 | (uint)(-v)) : (uint)v);
            PutU32(b, o, raw);
        }

        private static byte[] BuildGds(int ni, int nj, int la1, int lo1, int la2, int lo2,
            uint di, uint dj, byte scan, int template = 0)
        {
            var b = new byte[72];
            PutU32(b, 0, (uint)b.Length); // length
            b[4] = 3;                     // section number
            b[5] = 0;                     // source of grid def
            PutU32(b, 6, (uint)(ni * nj));// number of data points
            // octeto 13 (offset 12): número de plantilla (UInt16)
            b[12] = (byte)(template >> 8); b[13] = (byte)template;
            // plantilla 3.0 empieza en offset 14
            // offset 30 (octeto 31): Ni ; offset 34 (octeto 35): Nj
            PutU32(b, 30, (uint)ni);
            PutU32(b, 34, (uint)nj);
            PutI32Sm(b, 46, la1); // octeto 47
            PutI32Sm(b, 50, lo1); // octeto 51
            // b[54] resolución y componentes flags (octeto 55)
            PutI32Sm(b, 55, la2); // octeto 56
            PutI32Sm(b, 59, lo2); // octeto 60
            PutU32(b, 63, di);    // octeto 64
            PutU32(b, 67, dj);    // octeto 68
            b[71] = scan;         // octeto 72
            return b;
        }
    }
}
