using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.Validation;

namespace GribSharp.Tests
{
    [TestClass]
    public class Grib2ValidatorTests
    {
        private static byte[] Valid() => SyntheticGrib.BuildSimplePacked2x2(new float[] { 1, 2, 3, 4 });

        private static string SamplePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "gfs_sample.grib2");

        private static bool Has(Grib2ValidationResult r, Grib2ValidationCode code)
            => r.Issues.Any(i => i.Code == code);

        [TestMethod]
        public void ValidMessage_HasNoIssues()
        {
            var r = Grib2Validator.Validate(Valid());
            Assert.IsTrue(r.IsValid, r.ToString());
            Assert.AreEqual(0, r.Issues.Count, r.ToString());
            Assert.AreEqual(1, r.MessageCount);
            Assert.AreEqual(1, r.FieldCount);
        }

        [TestMethod]
        public void ValidMessage_DeepDecodeSucceeds()
        {
            var r = Grib2Validator.Validate(Valid(), Grib2ValidationOptions.Deep);
            Assert.IsTrue(r.IsValid, r.ToString());
            Assert.AreEqual(0, r.Issues.Count, r.ToString());
        }

        [TestMethod]
        public void IeeeMessage_ShortSection5_IsValid()
        {
            // La sección 5 de la plantilla 5.4 ocupa 12 octetos, no los 21 del
            // empaquetado simple: no debe confundirse con una sección truncada.
            var data = SyntheticGrib.BuildIeeeFloat2x2(new float[] { 1.5f, -2.5f, 3.25f, 4f });

            var r = Grib2Validator.Validate(data, Grib2ValidationOptions.Deep);
            Assert.IsTrue(r.IsValid, r.ToString());
            Assert.AreEqual(0, r.Issues.Count, r.ToString());
            CollectionAssert.AreEqual(
                new float[] { 1.5f, -2.5f, 3.25f, 4f },
                Grib2Parser.Parse(data)[0].Fields[0].Values);
        }

        [TestMethod]
        public void IeeeMessage_TruncatedData_IsDetected()
        {
            var data = SyntheticGrib.BuildIeeeFloat2x2(new float[] { 1, 2, 3, 4 });
            int drs = IndexOfSection(data, 5);
            data[drs + 8] = 8; // 8 valores IEEE requieren 32 bytes; sólo hay 16

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.DataSectionTooShort), r.ToString());
        }

        [TestMethod]
        public void EmptyInput_IsInvalid()
        {
            var r = Grib2Validator.Validate(new byte[0]);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.EmptyInput));
        }

        [TestMethod]
        public void NonGribInput_ReportsNoMessage()
        {
            var r = Grib2Validator.Validate(new byte[64]);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.NoMessageFound));
            Assert.AreEqual(0, r.MessageCount);
        }

        [TestMethod]
        public void TruncatedMessage_IsDetected()
        {
            var data = Valid();
            var cut = new byte[data.Length - 10];
            Array.Copy(data, cut, cut.Length);

            var r = Grib2Validator.Validate(cut);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.TruncatedMessage), r.ToString());
        }

        [TestMethod]
        public void MissingEndMarker_IsDetected()
        {
            var data = Valid();
            data[data.Length - 1] = (byte)'X';

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.MissingEndMarker), r.ToString());
        }

        [TestMethod]
        public void WrongEdition_IsDetected()
        {
            var data = Valid();
            data[7] = 1; // edición GRIB1

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.InvalidEdition), r.ToString());
        }

        [TestMethod]
        public void DeclaredLengthTooSmall_IsDetected()
        {
            var data = Valid();
            data[15] = 8; // longitud total = 8 (< 16)

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.InvalidMessageLength), r.ToString());
        }

        [TestMethod]
        public void SectionLengthOverflowingMessage_IsDetected()
        {
            var data = Valid();
            data[16] = 0x00; data[17] = 0x00; data[18] = 0x7F; data[19] = 0xFF; // sección 1 enorme

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.SectionOutOfBounds), r.ToString());
        }

        [TestMethod]
        public void ZeroSectionLength_IsDetected()
        {
            var data = Valid();
            data[16] = 0; data[17] = 0; data[18] = 0; data[19] = 0;

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.InvalidSectionLength), r.ToString());
        }

        [TestMethod]
        public void UnknownSectionNumber_IsDetected()
        {
            var data = Valid();
            data[20] = 9; // número de sección 9

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.UnknownSectionNumber), r.ToString());
        }

        [TestMethod]
        public void PointCountMismatch_IsDetected()
        {
            var data = Valid();
            int drs = IndexOfSection(data, 5);
            Assert.IsTrue(drs > 0, "sección 5 no encontrada");
            data[drs + 8] = 9; // número de valores empaquetados = 9 (rejilla de 4 puntos)

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.PointCountMismatch), r.ToString());
        }

        [TestMethod]
        public void DataSectionTooShort_IsDetected()
        {
            // 4 valores a 16 bits requieren 8 bytes; el mensaje sintético aporta 4.
            var data = Valid();
            int drs = IndexOfSection(data, 5);
            data[drs + 19] = 16; // bits por valor (octeto 20)

            var r = Grib2Validator.Validate(data);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(Has(r, Grib2ValidationCode.DataSectionTooShort), r.ToString());
        }

        [TestMethod]
        public void UnsupportedDataTemplate_IsWarningNotError()
        {
            var data = Valid();
            int drs = IndexOfSection(data, 5);
            data[drs + 9] = 0; data[drs + 10] = 50; // plantilla 5.50

            var r = Grib2Validator.Validate(data);
            Assert.IsTrue(r.IsValid, "una plantilla no soportada no compromete la integridad: " + r);
            Assert.IsTrue(Has(r, Grib2ValidationCode.UnsupportedTemplate), r.ToString());
        }

        [TestMethod]
        public void TrailingBytes_AreWarningNotError()
        {
            var data = Valid();
            var padded = new byte[data.Length + 5];
            Array.Copy(data, padded, data.Length);

            var r = Grib2Validator.Validate(padded);
            Assert.IsTrue(r.IsValid, r.ToString());
            Assert.IsTrue(Has(r, Grib2ValidationCode.TrailingGarbage), r.ToString());
        }

        [TestMethod]
        public void LeadingBytes_AreWarningNotError()
        {
            var data = Valid();
            var padded = new byte[data.Length + 3];
            Array.Copy(data, 0, padded, 3, data.Length);

            var r = Grib2Validator.Validate(padded);
            Assert.IsTrue(r.IsValid, r.ToString());
            Assert.IsTrue(Has(r, Grib2ValidationCode.LeadingGarbage), r.ToString());
        }

        [TestMethod]
        public void MultipleMessages_AreAllCounted()
        {
            var one = Valid();
            var two = new byte[one.Length * 2];
            Array.Copy(one, 0, two, 0, one.Length);
            Array.Copy(one, 0, two, one.Length, one.Length);

            var r = Grib2Validator.Validate(two);
            Assert.IsTrue(r.IsValid, r.ToString());
            Assert.AreEqual(2, r.MessageCount);
            Assert.AreEqual(2, r.FieldCount);
        }

        [TestMethod]
        public void MaxIssues_StopsScanning()
        {
            var data = new byte[64]; // sin mensajes: sólo produce NoMessageFound
            var r = Grib2Validator.Validate(data, new Grib2ValidationOptions { MaxIssues = 1 });
            Assert.IsTrue(r.IssueLimitReached);
            Assert.AreEqual(1, r.Issues.Count);
        }

        [TestMethod]
        public void CorruptInput_NeverThrows()
        {
            var rnd = new Random(1234);
            var data = Valid();
            for (int seed = 0; seed < 200; seed++)
            {
                var copy = (byte[])data.Clone();
                for (int k = 0; k < 5; k++)
                    copy[rnd.Next(copy.Length)] = (byte)rnd.Next(256);

                var r = Grib2Validator.Validate(copy, Grib2ValidationOptions.Deep);
                Assert.IsNotNull(r);
            }
        }

        [TestMethod]
        public void IsValid_Shortcut()
        {
            Assert.IsTrue(Grib2Validator.IsValid(Valid()));
            Assert.IsFalse(Grib2Validator.IsValid(new byte[32]));
        }

        [TestMethod]
        public void NullInput_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Grib2Validator.Validate((byte[])null));
            Assert.ThrowsException<ArgumentNullException>(() => Grib2Validator.Validate((Stream)null));
        }

        [TestMethod]
        public void GfsSample_IsValid()
        {
            if (!File.Exists(SamplePath))
                Assert.Inconclusive($"Fichero de muestra no encontrado: {SamplePath}");

            var r = Grib2Validator.ValidateFile(SamplePath, Grib2ValidationOptions.Deep);
            Assert.IsTrue(r.IsValid, r.ToString());
            Assert.IsTrue(r.MessageCount >= 1);
            Assert.AreEqual(0, r.WarningCount, r.ToString());
        }

        /// <summary>Desplazamiento del inicio de la primera sección con el número indicado.</summary>
        private static int IndexOfSection(byte[] data, int number)
        {
            int pos = 16; // tras la sección 0
            while (pos + 5 <= data.Length)
            {
                if (data[pos] == '7' && data[pos + 1] == '7') return -1;
                uint len = (uint)((data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3]);
                if (len < 5) return -1;
                if (data[pos + 4] == number) return pos;
                pos += (int)len;
            }
            return -1;
        }
    }
}
