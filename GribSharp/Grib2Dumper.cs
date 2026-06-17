using System.Collections.Generic;
using System.Text;
using GribSharp.Model;
using GribSharp.Tables;

namespace GribSharp
{
    /// <summary>Salida de texto para depuración (estilo wgrib2 -V).</summary>
    public static class Grib2Dumper
    {
        public static string Dump(byte[] data) => Dump(Grib2Parser.Parse(data));

        public static string Dump(IEnumerable<Grib2Message> messages)
        {
            var sb = new StringBuilder();
            int mi = 1;
            foreach (var m in messages)
            {
                sb.AppendLine($"== Message {mi} ==");
                sb.AppendLine($"  discipline={m.Discipline} edition={m.Edition} length={m.Length} center={CodeTables.Center(m.CenterId)}");
                int fi = 1;
                foreach (var f in m.Fields)
                {
                    DumpField(sb, fi, f);
                    fi++;
                }
                mi++;
            }
            return sb.ToString();
        }

        private static void DumpField(StringBuilder sb, int index, Grib2Field f)
        {
            float min = float.PositiveInfinity, max = float.NegativeInfinity;
            double sum = 0; int nan = 0, n = 0;
            foreach (var v in f.Values)
            {
                if (float.IsNaN(v)) { nan++; continue; }
                if (v < min) min = v;
                if (v > max) max = v;
                sum += v; n++;
            }
            double avg = n > 0 ? sum / n : 0;

            sb.AppendLine($"  -- Field {index} --");
            sb.AppendLine($"    discipline={f.DisciplineName} ({f.Discipline}) category={f.ParameterCategoryName} ({f.ParameterCategory})");
            sb.AppendLine($"    pdt={f.ProductDefinitionTemplateName} ({f.ProductDefinitionTemplate})");
            sb.AppendLine($"    param={f.ParameterName} [{f.Units}] (disc{f.Discipline}/cat{f.ParameterCategory}/num{f.ParameterNumber})");
            sb.AppendLine($"    level={f.LevelDescription} value={f.LevelValue} type={f.LevelType}");
            sb.AppendLine($"    refTime={f.ReferenceTime:yyyy-MM-dd HH:mm}Z forecast={f.ForecastTime}");
            sb.AppendLine($"    grid Ni={f.Grid.Ni} Nj={f.Grid.Nj} lat[{f.Grid.Lat1}..{f.Grid.Lat2}] lon[{f.Grid.Lon1}..{f.Grid.Lon2}] di={f.Grid.Di} dj={f.Grid.Dj} scan={f.Grid.ScanMode}");
            sb.AppendLine($"    values n={f.Values.Length} min={min} max={max} avg={avg:F4} nan={nan}");
        }
    }
}
