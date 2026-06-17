using GribSharp.IO;

namespace GribSharp.Sections
{
    public sealed class ProductDefinitionSection
    {
        public int Template;
        public int ParameterCategory;
        public int ParameterNumber;
        public int LevelType;
        public double LevelValue;
        public int ForecastTime;

        public static ProductDefinitionSection Read(Grib2Reader r, long sectionStart, uint length)
        {
            // r en octeto 6 (offset 5) tras SectionHeader.Read.
            r.ReadUInt16();                 // 6-7: NV (coordenadas verticales)
            int template = r.ReadUInt16();  // 8-9
            var s = new ProductDefinitionSection { Template = template };

            // 4.0 y 4.8 comparten el bloque inicial (octetos 10..). Leemos lo común.
            s.ParameterCategory = r.ReadUInt8(); // 10
            s.ParameterNumber = r.ReadUInt8();   // 11
            r.Skip(1);  // 12: tipo proceso generador
            r.Skip(1);  // 13: id proceso bg
            r.Skip(1);  // 14: id proceso analisis/forecast
            r.ReadUInt16(); // 15-16: horas corte
            r.Skip(1);  // 17: minutos corte
            r.Skip(1);  // 18: unidad de rango de tiempo
            s.ForecastTime = (int)r.ReadUInt32(); // 19-22
            s.LevelType = r.ReadUInt8();          // 23: tipo primera superficie fija
            int scaleFactor = r.ReadUInt8();      // 24
            int scaledValue = r.ReadInt32Sm();    // 25-28
            s.LevelValue = scaledValue / System.Math.Pow(10, scaleFactor);

            // Para 4.8 (intervalos) los campos de fin de intervalo siguen después;
            // no los necesitamos para identificar la variable. Saltamos al fin por longitud.
            r.Position = sectionStart + length;
            return s;
        }
    }
}
