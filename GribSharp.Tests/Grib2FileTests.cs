using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp;
using GribSharp.Model;

namespace GribSharp.Tests
{
    [TestClass]
    public class Grib2FileTests
    {
        private static Grib2File ParseSynthetic()
        {
            byte[] data = SyntheticGrib.BuildSimplePacked2x2(new float[] { 1, 2, 3, 4 });
            return Grib2Parser.ParseFile(data);
        }

        [TestMethod]
        public void ParseFile_ReturnsSameMessagesAsParse()
        {
            byte[] data = SyntheticGrib.BuildSimplePacked2x2(new float[] { 1, 2, 3, 4 });
            var direct = Grib2Parser.Parse(data);
            var file = Grib2Parser.ParseFile(data);
            Assert.AreEqual(direct.Count, file.Messages.Count);
            Assert.AreEqual(direct[0].Fields.Count, file.Messages[0].Fields.Count);
        }

        [TestMethod]
        public void ParameterNames_ContainsExpected()
        {
            var file = ParseSynthetic();
            Assert.IsTrue(file.ParameterNames.Contains("Temperature"));
        }

        [TestMethod]
        public void Fields_ContainsAllFields()
        {
            var file = ParseSynthetic();
            Assert.AreEqual(1, file.Fields.Count);
            Assert.AreEqual("Temperature", file.Fields[0].ParameterName);
        }

        [TestMethod]
        public void GetField_ByName_ReturnsField()
        {
            var file = ParseSynthetic();
            var f = file.GetField("Temperature");
            Assert.IsNotNull(f);
            Assert.AreEqual("Temperature", f.ParameterName);
        }

        [TestMethod]
        public void GetField_ByName_CaseInsensitive()
        {
            var file = ParseSynthetic();
            Assert.IsNotNull(file.GetField("temperature"));
            Assert.IsNotNull(file.GetField("TEMPERATURE"));
        }

        [TestMethod]
        public void GetField_ByName_NotFound_ReturnsNull()
        {
            var file = ParseSynthetic();
            Assert.IsNull(file.GetField("Nonexistent"));
        }

        [TestMethod]
        public void GetField_ByEnum_ReturnsField()
        {
            var file = ParseSynthetic();
            var f = file.GetField(Parameter.Temperature);
            Assert.IsNotNull(f);
            Assert.AreEqual("Temperature", f.ParameterName);
        }

        [TestMethod]
        public void GetField_ByEnum_NotFound_ReturnsNull()
        {
            var file = ParseSynthetic();
            Assert.IsNull(file.GetField(Parameter.RelativeHumidity));
        }

        [TestMethod]
        public void GetFields_ByName_ReturnsAll()
        {
            var file = ParseSynthetic();
            var fields = file.GetFields("Temperature");
            Assert.AreEqual(1, fields.Count);
        }

        [TestMethod]
        public void GetFields_ByName_NotFound_ReturnsEmpty()
        {
            var file = ParseSynthetic();
            var fields = file.GetFields("Nonexistent");
            Assert.AreEqual(0, fields.Count);
        }

        [TestMethod]
        public void TryGetField_Found_ReturnsTrue()
        {
            var file = ParseSynthetic();
            Assert.IsTrue(file.TryGetField("Temperature", out var f));
            Assert.IsNotNull(f);
        }

        [TestMethod]
        public void TryGetField_NotFound_ReturnsFalse()
        {
            var file = ParseSynthetic();
            Assert.IsFalse(file.TryGetField("Nonexistent", out var f));
            Assert.IsNull(f);
        }

        [TestMethod]
        public void TryGetField_ByEnum_Found_ReturnsTrue()
        {
            var file = ParseSynthetic();
            Assert.IsTrue(file.TryGetField(Parameter.Temperature, out var f));
            Assert.IsNotNull(f);
        }

        [TestMethod]
        public void TryGetField_ByEnum_NotFound_ReturnsFalse()
        {
            var file = ParseSynthetic();
            Assert.IsFalse(file.TryGetField(Parameter.WindSpeed, out var f));
            Assert.IsNull(f);
        }

        [TestMethod]
        public void Indexer_String_Works()
        {
            var file = ParseSynthetic();
            Assert.IsNotNull(file["Temperature"]);
            Assert.IsNull(file["Nonexistent"]);
        }

        [TestMethod]
        public void Indexer_Enum_Works()
        {
            var file = ParseSynthetic();
            Assert.IsNotNull(file[Parameter.Temperature]);
            Assert.IsNull(file[Parameter.Pressure]);
        }

        [TestMethod]
        public void GetField_WithLevel_MatchesCorrectField()
        {
            var file = ParseSynthetic();
            // Synthetic data: level type 1 (Ground), level value 0
            var f = file.GetField("Temperature", LevelType.GroundOrWaterSurface, 0);
            Assert.IsNotNull(f);
        }

        [TestMethod]
        public void GetField_WithLevel_WrongLevel_ReturnsNull()
        {
            var file = ParseSynthetic();
            Assert.IsNull(file.GetField("Temperature", LevelType.Isobaric, 500));
        }

        [TestMethod]
        public void KnownParameter_SetOnParsedField()
        {
            var file = ParseSynthetic();
            var f = file.Fields[0];
            Assert.AreEqual(Parameter.Temperature, f.KnownParameter);
            Assert.AreEqual(LevelType.GroundOrWaterSurface, f.KnownLevelType);
        }
    }
}
