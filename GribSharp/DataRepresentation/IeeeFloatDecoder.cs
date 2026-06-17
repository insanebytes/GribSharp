using GribSharp.IO;
using GribSharp.Sections;

namespace GribSharp.DataRepresentation
{
    /// <summary>Plantilla 5.4. Floats IEEE 754 crudos, big-endian.</summary>
    public sealed class IeeeFloatDecoder : IDataRepresentationDecoder
    {
        public float[] Decode(DataRepresentationSection drs, byte[] sectionData, int pointCount)
        {
            var r = new Grib2Reader(sectionData);
            var outVals = new float[pointCount];
            for (int i = 0; i < pointCount; i++)
                outVals[i] = r.ReadFloat32();
            return outVals;
        }
    }
}
