using GribSharp.Exceptions;
using GribSharp.IO;
using GribSharp.Model;

namespace GribSharp.Sections
{
    public sealed class GridDefinitionSection
    {
        public Grid Grid;

        public static GridDefinitionSection Read(Grib2Reader r, long sectionStart, uint length)
        {
            // r está en octeto 6 (offset 5) tras SectionHeader.Read.
            r.Skip(1);              // 6: fuente de la definición de rejilla
            r.ReadUInt32();         // 7-10: número de puntos
            r.Skip(1);              // 11: octetos para lista opcional
            r.Skip(1);              // 12: interpretación lista
            int template = r.ReadUInt16(); // 13-14
            if (template != 0)
                throw new GribNotSupportedException(3, template);

            r.Skip(1);   // 15: forma de la Tierra
            r.Skip(1);   // 16: factor escala radio esférico
            r.ReadUInt32(); // 17-20
            r.Skip(1);   // 21
            r.ReadUInt32(); // 22-25 eje mayor
            r.Skip(1);   // 26
            r.ReadUInt32(); // 27-30 eje menor
            int ni = (int)r.ReadUInt32(); // 31-34
            int nj = (int)r.ReadUInt32(); // 35-38
            r.ReadUInt32(); // 39-42 ángulo subdiv básico
            r.ReadUInt32(); // 43-46 subdiv
            double la1 = r.ReadInt32Sm() / 1e6; // 47-50
            double lo1 = r.ReadInt32Sm() / 1e6; // 51-54
            r.Skip(1);   // 55 resolución/componentes
            double la2 = r.ReadInt32Sm() / 1e6; // 56-59
            double lo2 = r.ReadInt32Sm() / 1e6; // 60-63
            double di = r.ReadUInt32() / 1e6;   // 64-67
            double dj = r.ReadUInt32() / 1e6;   // 68-71
            byte scan = r.ReadUInt8();          // 72

            var s = new GridDefinitionSection
            {
                Grid = new Grid
                {
                    Ni = ni, Nj = nj, Lat1 = la1, Lon1 = lo1, Lat2 = la2, Lon2 = lo2,
                    Di = di, Dj = dj, ScanMode = scan
                }
            };
            r.Position = sectionStart + length;
            return s;
        }
    }
}
