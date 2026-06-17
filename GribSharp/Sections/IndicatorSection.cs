using GribSharp.Exceptions;
using GribSharp.IO;

namespace GribSharp.Sections
{
    public sealed class IndicatorSection
    {
        public int Discipline;
        public int Edition;
        public long TotalLength;

        public static IndicatorSection Read(Grib2Reader r)
        {
            string magic = r.ReadAscii(4);
            if (magic != "GRIB")
                throw new GribFormatException($"Magic inválido: '{magic}'.");
            r.Skip(2); // reservado
            var s = new IndicatorSection { Discipline = r.ReadUInt8(), Edition = r.ReadUInt8() };
            if (s.Edition != 2)
                throw new GribFormatException($"Edición GRIB no soportada: {s.Edition} (se requiere 2).");
            s.TotalLength = (long)r.ReadUInt64();
            return s;
        }
    }
}
