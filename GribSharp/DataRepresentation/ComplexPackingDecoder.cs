using System;
using GribSharp.IO;
using GribSharp.Sections;

namespace GribSharp.DataRepresentation
{
    /// <summary>Plantillas 5.2 y 5.3 (complex packing, opcional spatial differencing).</summary>
    public sealed class ComplexPackingDecoder : IDataRepresentationDecoder
    {
        public float[] Decode(DataRepresentationSection drs, byte[] sectionData, int pointCount)
        {
            var t = new Grib2Reader(drs.RawTemplateBytes);
            t.Skip(1);                      // 22 splitting
            t.Skip(1);                      // 23 missing mgmt
            t.ReadUInt32();                 // 24-27 sustituto primario
            t.ReadUInt32();                 // 28-31 sustituto secundario
            int ng = (int)t.ReadUInt32();   // 32-35 número de grupos
            int refGroupWidth = t.ReadUInt8();      // 36
            int bitsGroupWidth = t.ReadUInt8();     // 37
            int refGroupLength = (int)t.ReadUInt32();// 38-41
            int lengthIncrement = t.ReadUInt8();    // 42
            int lastGroupLength = (int)t.ReadUInt32();// 43-46
            int bitsGroupLength = t.ReadUInt8();     // 47

            int order = 0, extraBytes = 0;
            if (drs.Template == 3)
            {
                order = t.ReadUInt8();      // 48
                extraBytes = t.ReadUInt8(); // 49
            }

            var br = new BitReader(sectionData);

            // Spatial differencing: valores extra al inicio (orden + valor mínimo global).
            long g1 = 0, gMin = 0;
            int hMin = 0;
            if (drs.Template == 3 && order > 0)
            {
                int extraBits = extraBytes * 8;
                g1 = br.ReadBits(extraBits);                          // primer valor
                if (order == 2) hMin = (int)br.ReadBits(extraBits);  // segundo valor
                gMin = SignedFromUnsigned(br.ReadBits(extraBits), extraBits); // mínimo de las diferencias
            }

            // Los tres sub-bloques (referencias, anchuras, longitudes) van alineados
            // a octeto entre sí en el empaquetado complejo NCEP.
            br.AlignToByte();

            // 1) Referencias de grupo: NG valores de bitsPerValue bits.
            var groupRefs = new long[ng];
            for (int i = 0; i < ng; i++) groupRefs[i] = br.ReadBits(drs.BitsPerValue);
            br.AlignToByte();

            // 2) Anchuras de grupo.
            var groupWidths = new int[ng];
            for (int i = 0; i < ng; i++)
                groupWidths[i] = refGroupWidth + (int)br.ReadBits(bitsGroupWidth);
            br.AlignToByte();

            // 3) Longitudes de grupo.
            var groupLengths = new int[ng];
            for (int i = 0; i < ng; i++)
                groupLengths[i] = refGroupLength + (int)br.ReadBits(bitsGroupLength) * lengthIncrement;
            groupLengths[ng - 1] = lastGroupLength;
            br.AlignToByte();

            // 4) Valores de cada grupo.
            var raw = new long[pointCount];
            int idx = 0;
            for (int g = 0; g < ng; g++)
            {
                int w = groupWidths[g];
                int len = groupLengths[g];
                for (int j = 0; j < len && idx < pointCount; j++)
                {
                    long v = w > 0 ? br.ReadBits(w) : 0;
                    raw[idx++] = groupRefs[g] + v;
                }
            }

            // 5) Deshacer spatial differencing.
            if (drs.Template == 3 && order > 0)
                UndoSpatialDifferencing(raw, order, g1, hMin, gMin);

            // 6) Escalado final.
            double dscale = Math.Pow(10, drs.DecimalScaleFactor);
            double bscale = Math.Pow(2, drs.BinaryScaleFactor);
            double rRef = drs.ReferenceValue;
            var outVals = new float[pointCount];
            for (int i = 0; i < pointCount; i++)
                outVals[i] = (float)((rRef + raw[i] * bscale) / dscale);
            return outVals;
        }

        private static void UndoSpatialDifferencing(long[] d, int order, long g1, int hMin, long gMin)
        {
            // Sumar el mínimo global de las diferencias.
            for (int i = 0; i < d.Length; i++) d[i] += gMin;

            if (order == 1)
            {
                d[0] = g1;
                for (int i = 1; i < d.Length; i++) d[i] += d[i - 1];
            }
            else if (order == 2)
            {
                // g1 = primer valor original, hMin = segundo valor original (ival2).
                d[0] = g1;
                d[1] = hMin;
                for (int i = 2; i < d.Length; i++)
                    d[i] = d[i] + 2 * d[i - 1] - d[i - 2];
            }
        }

        private static long SignedFromUnsigned(long raw, int bits)
        {
            // GRIB usa signed-magnitude para el mínimo de diferencias.
            long signBit = 1L << (bits - 1);
            if ((raw & signBit) != 0)
                return -(raw & (signBit - 1));
            return raw;
        }
    }
}
