using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Sections;

namespace GribSharp.Tests
{
    [TestClass]
    public class BitmapSectionTests
    {
        [TestMethod]
        public void AppliesBitmapNaN()
        {
            // bitmap 1010_0000 => puntos 0 y 2 presentes, 1 y 3 ausentes
            var bmp = new BitmapSection { Indicator = 0, HasBitmap = true, Bitmap = new byte[] { 0xA0 } };
            var decoded = new float[] { 5f, 7f }; // 2 valores presentes
            var full = BitmapApplier.Apply(decoded, bmp, 4);
            Assert.AreEqual(5f, full[0], 1e-6f);
            Assert.IsTrue(float.IsNaN(full[1]));
            Assert.AreEqual(7f, full[2], 1e-6f);
            Assert.IsTrue(float.IsNaN(full[3]));
        }

        [TestMethod]
        public void NoBitmapPassesThrough()
        {
            var bmp = new BitmapSection { Indicator = 255, HasBitmap = false };
            var decoded = new float[] { 1f, 2f, 3f };
            var full = BitmapApplier.Apply(decoded, bmp, 3);
            CollectionAssert.AreEqual(decoded, full);
        }
    }
}
