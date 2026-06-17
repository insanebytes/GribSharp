using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp;

namespace GribSharp.Tests
{
    [TestClass]
    public class Grib2ParserTests
    {
        [TestMethod]
        public void ParsesSyntheticSimplePackedMessage()
        {
            byte[] msg = SyntheticGrib.BuildSimplePacked2x2(new float[] { 1, 2, 3, 4 });
            var messages = Grib2Parser.Parse(msg);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(1, messages[0].Fields.Count);
            var f = messages[0].Fields[0];
            Assert.AreEqual(2, f.Grid.Ni);
            Assert.AreEqual(2, f.Grid.Nj);
            CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f }, f.Values);
        }
    }
}
