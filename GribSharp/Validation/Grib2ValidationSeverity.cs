namespace GribSharp.Validation
{
    /// <summary>Gravedad de una incidencia detectada al validar un fichero GRIB2.</summary>
    public enum Grib2ValidationSeverity
    {
        /// <summary>Anomalía que no impide leer el fichero (relleno, plantilla no soportada...).</summary>
        Warning = 0,

        /// <summary>El fichero está corrupto o incumple el formato: la lectura fallará o dará datos erróneos.</summary>
        Error = 1
    }
}
