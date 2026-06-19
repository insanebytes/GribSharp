using Microsoft.VisualStudio.TestTools.UnitTesting;
using GribSharp.DataRepresentation;
using GribSharp.Sections;
using GribSharp.Jpeg2000;

namespace GribSharp.Tests
{
    [TestClass]
    public class DecoderRegistryTests
    {
        private sealed class StubDecoder : IDataRepresentationDecoder
        {
            public float[] Decode(DataRepresentationSection drs, byte[] sectionData, int pointCount)
                => new float[pointCount];
        }

        [TestMethod]
        public void UnregisteredTemplateReturnsFalse()
        {
            // 9999: plantilla inexistente, nunca registrada.
            Assert.IsFalse(DataRepresentationDecoderRegistry.TryCreate(9999, out var d));
            Assert.IsNull(d);
        }

        [TestMethod]
        public void RegisteredTemplateIsResolved()
        {
            DataRepresentationDecoderRegistry.Register(9998, () => new StubDecoder());
            Assert.IsTrue(DataRepresentationDecoderRegistry.TryCreate(9998, out var d));
            Assert.IsInstanceOfType(d, typeof(StubDecoder));
        }

        [TestMethod]
        public void AddOnTypeResolvesByAssemblyQualifiedName()
        {
            // Blinda el string usado por la sonda por reflexión en Grib2Parser.
            var t = System.Type.GetType(
                "GribSharp.DataRepresentation.Jpeg2000Decoder, GribSharp.Jpeg2000");
            Assert.IsNotNull(t);
        }

        [TestMethod]
        public void Jpeg2000SupportRegistersTemplate40()
        {
            Jpeg2000Support.Register();
            Assert.IsTrue(DataRepresentationDecoderRegistry.TryCreate(40, out var d));
            Assert.IsInstanceOfType(d, typeof(Jpeg2000Decoder));
        }
    }
}
