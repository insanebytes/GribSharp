using System;

namespace GribSharp.Model
{
    public sealed class Grib2Field
    {
        public int Discipline;
        public int ParameterCategory;
        public int ParameterNumber;
        public string ParameterName;
        public string Units;

        public int LevelType;
        public double LevelValue;
        public string LevelDescription;

        public Parameter? KnownParameter;
        public LevelType? KnownLevelType;

        public DateTime ReferenceTime;
        public int ForecastTime;

        public Grid Grid;
        public float[] Values;

        /// <summary>Valor en lat/lon por vecino más cercano.</summary>
        public float GetValueAt(double lat, double lon)
        {
            int best = 0;
            double bestDist = double.MaxValue;
            for (int i = 0; i < Values.Length; i++)
            {
                var (plat, plon) = Grid.Coordinate(i);
                double d = (plat - lat) * (plat - lat) + (plon - lon) * (plon - lon);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return Values[best];
        }
    }
}
