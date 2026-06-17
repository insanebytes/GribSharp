using GribSharp.IO;

namespace GribSharp.Sections
{
    public sealed class DataSection
    {
        public byte[] Data;

        public static DataSection Read(Grib2Reader r, long sectionStart, uint length)
        {
            // r en octeto 6 (offset 5) tras SectionHeader.Read.
            int bytes = (int)(sectionStart + length - r.Position);
            var s = new DataSection { Data = r.ReadBytes(bytes) };
            r.Position = sectionStart + length;
            return s;
        }
    }
}
