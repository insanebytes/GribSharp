using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Model;
using GribSharp.Tables;

namespace GribSharp.Tests
{
    [TestClass]
    public class ParameterEnumTests
    {
        [TestMethod]
        public void ParameterCode_Temperature_Returns_0_0_0()
        {
            var (d, c, n) = CodeTables.ParameterCode(Parameter.Temperature);
            Assert.AreEqual(0, d);
            Assert.AreEqual(0, c);
            Assert.AreEqual(0, n);
        }

        [TestMethod]
        public void ParameterCode_GeopotentialHeight_Returns_0_3_5()
        {
            var (d, c, n) = CodeTables.ParameterCode(Parameter.GeopotentialHeight);
            Assert.AreEqual(0, d);
            Assert.AreEqual(3, c);
            Assert.AreEqual(5, n);
        }

        [TestMethod]
        public void ParameterCode_IceCover_Returns_10_2_0()
        {
            var (d, c, n) = CodeTables.ParameterCode(Parameter.IceCover);
            Assert.AreEqual(10, d);
            Assert.AreEqual(2, c);
            Assert.AreEqual(0, n);
        }

        [TestMethod]
        public void ToParameter_Known_ReturnsEnum()
        {
            var result = CodeTables.ToParameter(0, 1, 1);
            Assert.AreEqual(Parameter.RelativeHumidity, result);
        }

        [TestMethod]
        public void ToParameter_Unknown_ReturnsNull()
        {
            var result = CodeTables.ToParameter(0, 99, 99);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ToLevelType_Known_ReturnsEnum()
        {
            var result = CodeTables.ToLevelType(100);
            Assert.AreEqual(LevelType.Isobaric, result);
        }

        [TestMethod]
        public void ToLevelType_Unknown_ReturnsNull()
        {
            var result = CodeTables.ToLevelType(999);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AllParameterEnumValues_RoundTrip()
        {
            foreach (Parameter p in Enum.GetValues(typeof(Parameter)))
            {
                var (d, c, n) = CodeTables.ParameterCode(p);
                var back = CodeTables.ToParameter(d, c, n);
                Assert.AreEqual(p, back, $"Round-trip failed for {p}");
            }
        }

        [TestMethod]
        public void AllParameterEnumValues_MatchCodeTables()
        {
            foreach (Parameter p in Enum.GetValues(typeof(Parameter)))
            {
                var (d, c, n) = CodeTables.ParameterCode(p);
                var (name, units) = CodeTables.Parameter(d, c, n);
                Assert.IsFalse(name.StartsWith("disc"), $"Parameter {p} ({d},{c},{n}) not found in CodeTables: {name}");
                Assert.AreNotEqual("unknown", units, $"Parameter {p} has unknown units");
            }
        }
    }
}
