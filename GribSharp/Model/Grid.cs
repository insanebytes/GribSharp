namespace GribSharp.Model
{
    /// <summary>Rejilla lat/lon regular (plantilla 3.0). Scan mode bit-by-bit GRIB2.</summary>
    public sealed class Grid
    {
        public int Ni;
        public int Nj;
        public double Lat1, Lon1, Lat2, Lon2;
        public double Di, Dj;
        public byte ScanMode;

        public int PointCount => Ni * Nj;

        /// <summary>Lat/lon del punto en índice row-major. Soporta scan mode estándar GFS (0).</summary>
        public (double lat, double lon) Coordinate(int index)
        {
            int row = index / Ni;
            int col = index % Ni;

            bool iNegative = (ScanMode & 0x80) != 0;   // +i o -i
            bool jPositive = (ScanMode & 0x40) != 0;   // +j si set, si no -j

            double lon = iNegative ? Lon1 - col * Di : Lon1 + col * Di;
            double lat = jPositive ? Lat1 + row * Dj : Lat1 - row * Dj;
            return (lat, lon);
        }
    }
}
