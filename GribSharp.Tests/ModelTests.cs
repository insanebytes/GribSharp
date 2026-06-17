using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Model;

namespace GribSharp.Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void GridCoordinateRowMajorScan0()
        {
            // 0.25° global: 1440 x 721, lat 90..-90, lon 0..359.75
            var g = new Grid
            {
                Ni = 1440, Nj = 721,
                Lat1 = 90, Lon1 = 0, Lat2 = -90, Lon2 = 359.75,
                Di = 0.25, Dj = 0.25, ScanMode = 0
            };
            var (lat0, lon0) = g.Coordinate(0);
            Assert.AreEqual(90.0, lat0, 1e-9);
            Assert.AreEqual(0.0, lon0, 1e-9);
            var (lat1, lon1) = g.Coordinate(1);
            Assert.AreEqual(90.0, lat1, 1e-9);
            Assert.AreEqual(0.25, lon1, 1e-9);
            var (latRow1, lonRow1) = g.Coordinate(1440);
            Assert.AreEqual(89.75, latRow1, 1e-9);
            Assert.AreEqual(0.0, lonRow1, 1e-9);
        }

        [TestMethod]
        public void GetValueAtNearest()
        {
            var g = new Grid { Ni = 2, Nj = 2, Lat1 = 1, Lon1 = 0, Lat2 = 0, Lon2 = 1, Di = 1, Dj = 1, ScanMode = 0 };
            var f = new Grib2Field { Grid = g, Values = new float[] { 10, 11, 12, 13 } };
            Assert.AreEqual(10f, f.GetValueAt(1.0, 0.0), 1e-6f);
            Assert.AreEqual(13f, f.GetValueAt(0.0, 1.0), 1e-6f);
        }
    }
}
