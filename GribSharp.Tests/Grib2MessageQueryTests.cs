using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp;
using GribSharp.Model;

namespace GribSharp.Tests
{
    [TestClass]
    public class Grib2MessageQueryTests
    {
        private static Grib2Message ParseSyntheticMessage()
        {
            byte[] data = SyntheticGrib.BuildSimplePacked2x2(new float[] { 1, 2, 3, 4 });
            return Grib2Parser.Parse(data)[0];
        }

        [TestMethod]
        public void FieldNames_ReturnsSortedDistinctNames()
        {
            var msg = ParseSyntheticMessage();
            var names = msg.FieldNames;
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual("Temperature", names[0]);
        }

        [TestMethod]
        public void GetField_ByName_ReturnsFirstMatch()
        {
            var msg = ParseSyntheticMessage();
            var f = msg.GetField("Temperature");
            Assert.IsNotNull(f);
            Assert.AreEqual("Temperature", f.ParameterName);
        }

        [TestMethod]
        public void GetField_ByName_CaseInsensitive()
        {
            var msg = ParseSyntheticMessage();
            Assert.IsNotNull(msg.GetField("temperature"));
        }

        [TestMethod]
        public void GetField_ByName_NotFound_ReturnsNull()
        {
            var msg = ParseSyntheticMessage();
            Assert.IsNull(msg.GetField("Nonexistent"));
        }

        [TestMethod]
        public void GetFields_ByName_ReturnsAllMatches()
        {
            var msg = ParseSyntheticMessage();
            var fields = msg.GetFields("Temperature");
            Assert.AreEqual(1, fields.Count);
        }

        [TestMethod]
        public void GetField_ByEnum_ReturnsMatch()
        {
            var msg = ParseSyntheticMessage();
            var f = msg.GetField(Parameter.Temperature);
            Assert.IsNotNull(f);
        }

        [TestMethod]
        public void GetField_ByEnum_NotFound_ReturnsNull()
        {
            var msg = ParseSyntheticMessage();
            Assert.IsNull(msg.GetField(Parameter.RelativeHumidity));
        }

        [TestMethod]
        public void TryGetField_ReturnsTrueWhenFound()
        {
            var msg = ParseSyntheticMessage();
            Assert.IsTrue(msg.TryGetField("Temperature", out var f));
            Assert.IsNotNull(f);
        }

        [TestMethod]
        public void TryGetField_ReturnsFalseWhenNotFound()
        {
            var msg = ParseSyntheticMessage();
            Assert.IsFalse(msg.TryGetField("Nonexistent", out var f));
            Assert.IsNull(f);
        }

        [TestMethod]
        public void TryGetField_ByEnum_ReturnsTrueWhenFound()
        {
            var msg = ParseSyntheticMessage();
            Assert.IsTrue(msg.TryGetField(Parameter.Temperature, out var f));
            Assert.IsNotNull(f);
        }

        [TestMethod]
        public void TryGetField_ByEnum_ReturnsFalseWhenNotFound()
        {
            var msg = ParseSyntheticMessage();
            Assert.IsFalse(msg.TryGetField(Parameter.Pressure, out var f));
            Assert.IsNull(f);
        }
    }
}
