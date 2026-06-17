using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp;

namespace GribSharp.Tests
{
    [TestClass]
    public class DumperTests
    {
        [TestMethod]
        public void DumpContainsKeyFields()
        {
            byte[] msg = SyntheticGrib.BuildSimplePacked2x2(new float[] { 1, 2, 3, 4 });
            string text = Grib2Dumper.Dump(msg);
            StringAssert.Contains(text, "Message 1");
            StringAssert.Contains(text, "Ni=2");
            StringAssert.Contains(text, "Nj=2");
            StringAssert.Contains(text, "min=");
            StringAssert.Contains(text, "max=");
        }
    }
}
