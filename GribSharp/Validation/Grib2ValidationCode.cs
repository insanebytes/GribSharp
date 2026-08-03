namespace GribSharp.Validation
{
    /// <summary>Identificador estable de cada tipo de incidencia, para tratarlas mediante código.</summary>
    public enum Grib2ValidationCode
    {
        /// <summary>Entrada vacía.</summary>
        EmptyInput,

        /// <summary>No se encontró ningún mensaje GRIB (falta la marca 'GRIB').</summary>
        NoMessageFound,

        /// <summary>Bytes ajenos al formato antes de un mensaje.</summary>
        LeadingGarbage,

        /// <summary>Bytes sobrantes tras el último mensaje.</summary>
        TrailingGarbage,

        /// <summary>El fichero acaba antes de completar la estructura anunciada.</summary>
        UnexpectedEndOfFile,

        /// <summary>Edición GRIB distinta de 2.</summary>
        InvalidEdition,

        /// <summary>La longitud declarada en la sección 0 es imposible.</summary>
        InvalidMessageLength,

        /// <summary>El mensaje declara más bytes de los que quedan en el fichero.</summary>
        TruncatedMessage,

        /// <summary>Falta el marcador final '7777'.</summary>
        MissingEndMarker,

        /// <summary>Longitud de sección inválida (menor que la cabecera).</summary>
        InvalidSectionLength,

        /// <summary>La sección se sale del final del mensaje.</summary>
        SectionOutOfBounds,

        /// <summary>Número de sección fuera del rango 1-7.</summary>
        UnknownSectionNumber,

        /// <summary>Las secciones no aparecen en el orden que exige el formato.</summary>
        SectionOutOfOrder,

        /// <summary>Falta una sección obligatoria.</summary>
        MissingSection,

        /// <summary>Discrepancia entre los puntos de la rejilla, la sección 5 y el bitmap.</summary>
        PointCountMismatch,

        /// <summary>El bitmap no cubre todos los puntos de la rejilla.</summary>
        BitmapTooShort,

        /// <summary>La sección 7 contiene menos bytes de los que exige el empaquetado.</summary>
        DataSectionTooShort,

        /// <summary>Plantilla correcta según el formato pero no soportada por la librería.</summary>
        UnsupportedTemplate,

        /// <summary>La decodificación de los datos falló (sólo en validación profunda).</summary>
        DecodeFailure,

        /// <summary>Error inesperado al leer el flujo de entrada.</summary>
        ReadFailure
    }
}
