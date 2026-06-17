using GribSharp.IO;

namespace GribSharp.Sections
{
    public sealed class DataRepresentationSection
    {
        public int Template;
        public int DataPointCount;
        public float ReferenceValue;
        public int BinaryScaleFactor;
        public int DecimalScaleFactor;
        public int BitsPerValue;
        public int OriginalFieldType;
        public byte[] RawTemplateBytes;

        public static DataRepresentationSection Read(Grib2Reader r, long sectionStart, uint length)
        {
            // r en octeto 6 (offset 5) tras SectionHeader.Read.
            var s = new DataRepresentationSection();
            s.DataPointCount = (int)r.ReadUInt32(); // 6-9
            s.Template = r.ReadUInt16();            // 10-11
            // Campos comunes a 5.0/5.2/5.3 (octetos 12..21)
            s.ReferenceValue = r.ReadFloat32();     // 12-15 (IEEE)
            s.BinaryScaleFactor = r.ReadInt16Sm();  // 16-17
            s.DecimalScaleFactor = r.ReadInt16Sm(); // 18-19
            s.BitsPerValue = r.ReadUInt8();         // 20
            s.OriginalFieldType = r.ReadUInt8();    // 21
            // Resto del template (sólo presente en 5.2/5.3) para decoders complejos.
            long remaining = (sectionStart + length) - r.Position;
            s.RawTemplateBytes = remaining > 0 ? r.ReadBytes((int)remaining) : new byte[0];
            r.Position = sectionStart + length;
            return s;
        }
    }
}
