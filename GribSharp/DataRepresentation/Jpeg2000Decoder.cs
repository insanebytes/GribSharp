using System;
using GribSharp.Sections;

namespace GribSharp.DataRepresentation
{
    /// <summary>Plantilla 5.40. Decodifica JPEG2000 con CSJ2K y aplica escalado GRIB.</summary>
    public sealed class Jpeg2000Decoder : IDataRepresentationDecoder
    {
        private readonly Func<byte[], int[]> _decodeCodestream;

        public Jpeg2000Decoder() : this(DecodeWithCsj2k) { }

        /// <summary>Constructor de prueba: permite inyectar el decodificador de codestream.</summary>
        public Jpeg2000Decoder(Func<byte[], int[]> decodeCodestream)
        {
            _decodeCodestream = decodeCodestream;
        }

        public float[] Decode(DataRepresentationSection drs, byte[] sectionData, int pointCount)
        {
            double dscale = Math.Pow(10, drs.DecimalScaleFactor);
            double bscale = Math.Pow(2, drs.BinaryScaleFactor);
            double r = drs.ReferenceValue;
            var outVals = new float[pointCount];

            if (drs.BitsPerValue == 0 || sectionData.Length == 0)
            {
                float constant = (float)(r / dscale);
                for (int i = 0; i < pointCount; i++) outVals[i] = constant;
                return outVals;
            }

            int[] x = _decodeCodestream(sectionData);
            for (int i = 0; i < pointCount; i++)
            {
                long xi = i < x.Length ? x[i] : 0;
                outVals[i] = (float)((r + xi * bscale) / dscale);
            }
            return outVals;
        }

        // CSJ2K 3.0.0: J2kImage.FromBytes(byte[], ParameterList) -> PortableImage;
        // PortableImage.GetComponent(0) -> int[] (componente único, escala de grises).
        private static int[] DecodeWithCsj2k(byte[] codestream)
        {
            var img = CSJ2K.J2kImage.FromBytes(codestream, null);
            return img.GetComponent(0);
        }
    }
}
