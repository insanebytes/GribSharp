using System;

namespace GribSharp.Model
{
    public sealed class Grib2Field
    {
        public int Discipline;
        public string DisciplineName;
        public int ParameterCategory;
        public string ParameterCategoryName;
        public int ParameterNumber;
        public string ParameterName;
        public string Units;

        /// <summary>Product Definition Template number (GRIB2 Code Table 4.0).</summary>
        public int ProductDefinitionTemplate;
        public string ProductDefinitionTemplateName;

        public int LevelType;
        public double LevelValue;
        public string LevelDescription;

        public Discipline? KnownDiscipline;
        public ProductDefinitionTemplate? KnownProductDefinitionTemplate;
        public Parameter? KnownParameter;
        public LevelType? KnownLevelType;

        public DateTime ReferenceTime;
        public int ForecastTime;

        public Grid Grid;
        public float[] Values;

        /// <summary>
        /// Valor en lat/lon por interpolación bilineal entre los cuatro puntos de rejilla
        /// que rodean la coordenada (igual que xyGrib). Si algún vecino es NaN (sin dato),
        /// recurre al vecino más cercano.
        /// </summary>
        public float GetValueAt(double lat, double lon)
        {
            var (frow, fcol) = Grid.FractionalPosition(lat, lon);

            int row0 = (int)Math.Floor(frow);
            int col0 = (int)Math.Floor(fcol);
            double tr = frow - row0; // peso hacia row0+1
            double tc = fcol - col0; // peso hacia col0+1

            float v00 = Values[Grid.IndexOf(row0, col0)];
            float v01 = Values[Grid.IndexOf(row0, col0 + 1)];
            float v10 = Values[Grid.IndexOf(row0 + 1, col0)];
            float v11 = Values[Grid.IndexOf(row0 + 1, col0 + 1)];

            // Datos ausentes (bitmap) → no interpolar, usar vecino más cercano.
            if (float.IsNaN(v00) || float.IsNaN(v01) || float.IsNaN(v10) || float.IsNaN(v11))
                return GetNearestValueAt(lat, lon);

            float top = (float)(v00 * (1 - tc) + v01 * tc);
            float bottom = (float)(v10 * (1 - tc) + v11 * tc);
            return (float)(top * (1 - tr) + bottom * tr);
        }

        /// <summary>Valor en lat/lon por vecino más cercano.</summary>
        public float GetNearestValueAt(double lat, double lon)
        {
            int best = 0;
            double bestDist = double.MaxValue;
            for (int i = 0; i < Values.Length; i++)
            {
                if (float.IsNaN(Values[i])) continue;
                var (plat, plon) = Grid.Coordinate(i);
                double d = (plat - lat) * (plat - lat) + (plon - lon) * (plon - lon);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return Values[best];
        }
    }
}
