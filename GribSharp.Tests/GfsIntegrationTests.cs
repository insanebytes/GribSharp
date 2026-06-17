using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp;
using GribSharp.Model;

namespace GribSharp.Tests
{
    [TestClass]
    public class GfsIntegrationTests
    {
        private static string SamplePath =>
            Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", "gfs_sample.grib2");

        private static byte[] LoadOrSkip()
        {
            if (!File.Exists(SamplePath))
            {
                Assert.Inconclusive($"Fichero de muestra no encontrado: {SamplePath}. " +
                    "Descargar de NOMADS (var TMP, lev 2 m above ground) para ejecutar.");
            }
            return File.ReadAllBytes(SamplePath);
        }

        [TestMethod]
        public void ParsesGfsSample_GridDimensions()
        {
            byte[] data = LoadOrSkip();
            var messages = Grib2Parser.Parse(data);
            Assert.IsTrue(messages.Count >= 1);
            var f = messages[0].Fields[0];
            Assert.AreEqual(1440, f.Grid.Ni);
            Assert.AreEqual(721, f.Grid.Nj);
            Assert.AreEqual(90.0, f.Grid.Lat1, 1e-6);
            Assert.AreEqual(0.25, f.Grid.Di, 1e-9);
            Assert.AreEqual(1440 * 721, f.Values.Length);
        }

        [TestMethod]
        public void DecodesPlausibleTemperature()
        {
            byte[] data = LoadOrSkip();
            var f = Grib2Parser.Parse(data)[0].Fields[0];
            // TMP a 2 m en Kelvin: valores físicamente plausibles globalmente.
            var valid = f.Values.Where(v => !float.IsNaN(v)).ToArray();
            Assert.IsTrue(valid.Length > 0, "sin valores válidos");
            Assert.IsTrue(valid.Min() > 180f, $"min={valid.Min()}");
            Assert.IsTrue(valid.Max() < 340f, $"max={valid.Max()}");
        }

        [TestMethod]
        public void DumpProducesText()
        {
            byte[] data = LoadOrSkip();
            string text = Grib2Dumper.Dump(data);
            StringAssert.Contains(text, "grid Ni=1440");
        }

        [TestMethod]
        public void ParseFile_DiscoverParameterNames()
        {
            byte[] data = LoadOrSkip();
            var file = Grib2Parser.ParseFile(data);
            Assert.IsTrue(file.ParameterNames.Count > 0, "No parameter names found");
        }

        [TestMethod]
        public void ParseFile_GetFieldByName_Temperature()
        {
            byte[] data = LoadOrSkip();
            var file = Grib2Parser.ParseFile(data);
            var f = file.GetField("Temperature");
            Assert.IsNotNull(f, "Temperature field not found");
            Assert.AreEqual("K", f.Units);
        }

        [TestMethod]
        public void ParseFile_GetFieldByEnum_Temperature()
        {
            byte[] data = LoadOrSkip();
            var file = Grib2Parser.ParseFile(data);
            var byEnum = file.GetField(Parameter.Temperature);
            var byName = file.GetField("Temperature");
            Assert.IsNotNull(byEnum);
            Assert.AreSame(byName, byEnum);
        }

        [TestMethod]
        public void ParseFile_GetFieldWithLevel()
        {
            byte[] data = LoadOrSkip();
            var file = Grib2Parser.ParseFile(data);
            var f = file.GetField(Parameter.Temperature, LevelType.HeightAboveGround, 2);
            if (f == null)
                Assert.Inconclusive("Temperature at 2m above ground not in sample file");
            Assert.AreEqual(2.0, f.LevelValue, 0.5);
        }
    }
}
