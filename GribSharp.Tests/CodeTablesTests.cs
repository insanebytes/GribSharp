using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Tables;

namespace GribSharp.Tests
{
    [TestClass]
    public class CodeTablesTests
    {
        [TestMethod]
        public void ResolvesKnownParameter()
        {
            var (name, units) = CodeTables.Parameter(0, 2, 2);
            Assert.AreEqual("U-component of wind", name);
            Assert.AreEqual("m/s", units);
        }

        [TestMethod]
        public void UnknownParameterGenericName()
        {
            var (name, units) = CodeTables.Parameter(0, 99, 99);
            Assert.AreEqual("disc0/cat99/num99", name);
            Assert.AreEqual("unknown", units);
        }

        [TestMethod]
        public void ResolvesLevelAndCenter()
        {
            Assert.AreEqual("Isobaric surface", CodeTables.LevelDescription(100));
            Assert.AreEqual("US National Weather Service - NCEP", CodeTables.Center(7));
        }
    }
}
