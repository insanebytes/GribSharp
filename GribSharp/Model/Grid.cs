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

        private bool INegative => (ScanMode & 0x80) != 0;   // +i o -i
        private bool JPositive => (ScanMode & 0x40) != 0;   // +j si set, si no -j

        /// <summary>Lat/lon del punto en índice row-major. Soporta scan mode estándar GFS (0).</summary>
        public (double lat, double lon) Coordinate(int index)
        {
            int row = index / Ni;
            int col = index % Ni;

            double lon = INegative ? Lon1 - col * Di : Lon1 + col * Di;
            double lat = JPositive ? Lat1 + row * Dj : Lat1 - row * Dj;
            return (lat, lon);
        }

        /// <summary>Posición fraccionaria (fila, columna) de una lat/lon. Inversa de <see cref="Coordinate"/>.</summary>
        public (double row, double col) FractionalPosition(double lat, double lon)
        {
            // Normaliza la longitud al rango de la rejilla para soportar consultas en [-180,180] o [0,360).
            double lonOffset = lon - Lon1;
            double span = Ni * Di; // ancho total en grados (360 para rejilla global)
            if (span > 0)
            {
                lonOffset %= span;
                if (lonOffset < 0) lonOffset += span;
            }

            double col = INegative ? -lonOffset / Di : lonOffset / Di;
            double row = JPositive ? (lat - Lat1) / Dj : (Lat1 - lat) / Dj;
            return (row, col);
        }

        /// <summary>Índice row-major a partir de fila/columna, con envoltura en i (longitud) y recorte en j (latitud).</summary>
        public int IndexOf(int row, int col)
        {
            if (row < 0) row = 0;
            else if (row > Nj - 1) row = Nj - 1;

            col %= Ni;
            if (col < 0) col += Ni;

            return row * Ni + col;
        }
    }
}
