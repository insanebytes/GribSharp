using GribSharp.IO;

namespace GribSharp.Sections
{
    public struct SectionHeader
    {
        public uint Length;
        public int Number;

        /// <summary>Lee length(4)+number(1) y avanza el lector 5 bytes.</summary>
        public static SectionHeader Read(Grib2Reader r)
        {
            return new SectionHeader { Length = r.ReadUInt32(), Number = r.ReadUInt8() };
        }

        /// <summary>Inspecciona length(4)+number(1) sin avanzar el lector.</summary>
        public static SectionHeader Peek(Grib2Reader r)
        {
            long pos = r.Position;
            var h = Read(r);
            r.Position = pos;
            return h;
        }
    }
}
