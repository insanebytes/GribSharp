using System.Collections.Generic;

namespace GribSharp.Tables
{
    /// <summary>
    /// Tablas de código GRIB2 (WMO + locales NCEP). Cubre los parámetros habituales de
    /// GFS pgrb2 0.25°. Cualquier (discipline, category, number) no listado se resuelve
    /// con un nombre genérico, de modo que TODA variable se parsea sin excepción.
    /// </summary>
    public static class CodeTables
    {
        private static readonly Dictionary<(int, int, int), (string, string)> Params =
            new Dictionary<(int, int, int), (string, string)>
        {
            // Disciplina 0 - Meteorología
            // Categoría 0: Temperatura
            { (0,0,0), ("Temperature", "K") },
            { (0,0,1), ("Virtual temperature", "K") },
            { (0,0,2), ("Potential temperature", "K") },
            { (0,0,4), ("Maximum temperature", "K") },
            { (0,0,5), ("Minimum temperature", "K") },
            { (0,0,6), ("Dewpoint temperature", "K") },
            { (0,0,7), ("Dewpoint depression", "K") },
            { (0,0,9), ("Temperature anomaly", "K") },
            { (0,0,10), ("Latent heat net flux", "W/m^2") },
            { (0,0,11), ("Sensible heat net flux", "W/m^2") },
            { (0,0,21), ("Apparent temperature", "K") },
            // Categoría 1: Humedad
            { (0,1,0), ("Specific humidity", "kg/kg") },
            { (0,1,1), ("Relative humidity", "%") },
            { (0,1,2), ("Humidity mixing ratio", "kg/kg") },
            { (0,1,3), ("Precipitable water", "kg/m^2") },
            { (0,1,7), ("Precipitation rate", "kg/m^2/s") },
            { (0,1,8), ("Total precipitation", "kg/m^2") },
            { (0,1,9), ("Large-scale precipitation", "kg/m^2") },
            { (0,1,10), ("Convective precipitation", "kg/m^2") },
            { (0,1,11), ("Snow depth", "m") },
            { (0,1,13), ("Water equivalent of accumulated snow depth", "kg/m^2") },
            { (0,1,22), ("Cloud mixing ratio", "kg/kg") },
            { (0,1,29), ("Total snowfall", "m") },
            { (0,1,39), ("Percent frozen precipitation", "%") },
            { (0,1,49), ("Total water precipitation", "kg/m^2") },
            // Categoría 2: Momento (viento)
            { (0,2,0), ("Wind direction", "deg") },
            { (0,2,1), ("Wind speed", "m/s") },
            { (0,2,2), ("U-component of wind", "m/s") },
            { (0,2,3), ("V-component of wind", "m/s") },
            { (0,2,8), ("Vertical velocity (pressure)", "Pa/s") },
            { (0,2,9), ("Vertical velocity (geometric)", "m/s") },
            { (0,2,10), ("Absolute vorticity", "1/s") },
            { (0,2,14), ("Potential vorticity", "K m^2/kg/s") },
            { (0,2,22), ("Wind gust", "m/s") },
            // Categoría 3: Masa (presión)
            { (0,3,0), ("Pressure", "Pa") },
            { (0,3,1), ("Pressure reduced to MSL", "Pa") },
            { (0,3,2), ("Pressure tendency", "Pa/s") },
            { (0,3,3), ("ICAO Standard Atmosphere reference height", "m") },
            { (0,3,4), ("Geopotential", "m^2/s^2") },
            { (0,3,5), ("Geopotential height", "gpm") },
            { (0,3,6), ("Geometric height", "m") },
            { (0,3,9), ("Geopotential height anomaly", "gpm") },
            { (0,3,10), ("Density", "kg/m^3") },
            { (0,3,11), ("Altimeter setting", "Pa") },
            // Categoría 4: Radiación de onda corta
            { (0,4,7), ("Downward short-wave radiation flux", "W/m^2") },
            { (0,4,8), ("Upward short-wave radiation flux", "W/m^2") },
            { (0,4,9), ("Net short-wave radiation flux", "W/m^2") },
            // Categoría 5: Radiación de onda larga
            { (0,5,3), ("Downward long-wave radiation flux", "W/m^2") },
            { (0,5,4), ("Upward long-wave radiation flux", "W/m^2") },
            { (0,5,5), ("Net long-wave radiation flux", "W/m^2") },
            // Categoría 6: Nubes
            { (0,6,1), ("Total cloud cover", "%") },
            { (0,6,3), ("Low cloud cover", "%") },
            { (0,6,4), ("Medium cloud cover", "%") },
            { (0,6,5), ("High cloud cover", "%") },
            { (0,6,6), ("Cloud water", "kg/m^2") },
            { (0,6,22), ("Cloud cover", "%") },
            // Categoría 7: Estabilidad termodinámica
            { (0,7,6), ("Convective available potential energy", "J/kg") },
            { (0,7,7), ("Convective inhibition", "J/kg") },
            { (0,7,8), ("Storm relative helicity", "m^2/s^2") },
            // Categoría 19: Físico
            { (0,19,0), ("Visibility", "m") },
            { (0,19,1), ("Albedo", "%") },

            // Disciplina 2 - Superficie terrestre
            { (2,0,0), ("Land cover (1=land, 0=sea)", "proportion") },
            { (2,0,1), ("Surface roughness", "m") },
            { (2,0,2), ("Soil temperature", "K") },
            { (2,0,3), ("Soil moisture content", "kg/m^2") },
            { (2,0,5), ("Water runoff", "kg/m^2") },
            { (2,0,22), ("Volumetric soil moisture content", "proportion") },

            // Disciplina 10 - Oceanografía
            { (10,2,0), ("Ice cover", "proportion") },
            { (10,2,1), ("Ice thickness", "m") },
            { (10,3,0), ("Water temperature", "K") },
        };

        private static readonly Dictionary<int, string> Levels = new Dictionary<int, string>
        {
            { 1, "Ground or water surface" },
            { 2, "Cloud base" },
            { 3, "Cloud top" },
            { 4, "0 degC isotherm" },
            { 6, "Maximum wind level" },
            { 7, "Tropopause" },
            { 100, "Isobaric surface" },
            { 101, "Mean sea level" },
            { 102, "Specific altitude above mean sea level" },
            { 103, "Height above ground" },
            { 104, "Sigma level" },
            { 105, "Hybrid level" },
            { 106, "Depth below land surface" },
            { 108, "Level at specified pressure difference from ground" },
            { 109, "Potential vorticity surface" },
            { 200, "Entire atmosphere" },
            { 220, "Planetary boundary layer" },
        };

        private static readonly Dictionary<int, string> Centers = new Dictionary<int, string>
        {
            { 7, "US NCEP" },
            { 8, "US NWSTG" },
            { 9, "US other" },
            { 98, "ECMWF" },
        };

        public static (string name, string units) Parameter(int discipline, int category, int number)
        {
            if (Params.TryGetValue((discipline, category, number), out var v))
                return v;
            return ($"disc{discipline}/cat{category}/num{number}", "unknown");
        }

        public static string LevelDescription(int levelType)
            => Levels.TryGetValue(levelType, out var d) ? d : $"level type {levelType}";

        public static string Center(int centerId)
            => Centers.TryGetValue(centerId, out var c) ? c : $"center {centerId}";

        public static (int discipline, int category, int number) ParameterCode(Model.Parameter parameter)
        {
            int v = (int)parameter;
            return (v / 1_000_000, (v / 1_000) % 1_000, v % 1_000);
        }

        public static Model.Parameter? ToParameter(int discipline, int category, int number)
        {
            int encoded = discipline * 1_000_000 + category * 1_000 + number;
            if (System.Enum.IsDefined(typeof(Model.Parameter), encoded))
                return (Model.Parameter)encoded;
            return null;
        }

        public static Model.LevelType? ToLevelType(int levelTypeCode)
        {
            if (System.Enum.IsDefined(typeof(Model.LevelType), levelTypeCode))
                return (Model.LevelType)levelTypeCode;
            return null;
        }
    }
}
