namespace GribSharp.Validation
{
    /// <summary>Ajustes de la comprobación de integridad.</summary>
    public sealed class Grib2ValidationOptions
    {
        /// <summary>
        /// Decodifica los datos de cada campo además de comprobar la estructura.
        /// Detecta corrupción interna del empaquetado a costa de leer todo el fichero.
        /// </summary>
        public bool DecodeData { get; set; }

        /// <summary>
        /// Máximo de incidencias a acumular. Evita que un fichero completamente
        /// corrupto genere millones de entradas. Por defecto 200; 0 o negativo = sin límite.
        /// </summary>
        public int MaxIssues { get; set; } = 200;

        /// <summary>Validación estructural rápida (sin decodificar datos).</summary>
        public static Grib2ValidationOptions Default => new Grib2ValidationOptions();

        /// <summary>Validación estructural más decodificación completa de cada campo.</summary>
        public static Grib2ValidationOptions Deep => new Grib2ValidationOptions { DecodeData = true };
    }
}
