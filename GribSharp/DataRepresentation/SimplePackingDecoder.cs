using System;
using GribSharp.IO;
using GribSharp.Sections;

namespace GribSharp.DataRepresentation
{
    /// <summary>Plantilla 5.0. value = (R + X * 2^E) / 10^D.</summary>
    public sealed class SimplePackingDecoder : IDataRepresentationDecoder
    {
        public float[] Decode(DataRepresentationSection drs, byte[] sectionData, int pointCount)
        {
            var outVals = new float[pointCount];
            double dscale = Math.Pow(10, drs.DecimalScaleFactor);
            double bscale = Math.Pow(2, drs.BinaryScaleFactor);
            double r = drs.ReferenceValue;

            if (drs.BitsPerValue == 0)
            {
                float constant = (float)(r / dscale);
                for (int i = 0; i < pointCount; i++) outVals[i] = constant;
                return outVals;
            }

            var br = new BitReader(sectionData);
            for (int i = 0; i < pointCount; i++)
            {
                long x = br.ReadBits(drs.BitsPerValue);
                outVals[i] = (float)((r + x * bscale) / dscale);
            }
            return outVals;
        }
    }
}
