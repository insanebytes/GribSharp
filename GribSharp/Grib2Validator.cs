using System;
using System.IO;
using GribSharp.DataRepresentation;
using GribSharp.Exceptions;
using GribSharp.IO;
using GribSharp.Sections;
using GribSharp.Validation;

namespace GribSharp
{
    /// <summary>
    /// Comprobación de integridad de ficheros GRIB2. A diferencia de
    /// <see cref="Grib2Parser"/>, no lanza excepciones ante datos corruptos: recorre
    /// la estructura hasta donde puede y devuelve todas las anomalías encontradas.
    /// </summary>
    public static class Grib2Validator
    {
        /// <summary>Valida el fichero indicado.</summary>
        public static Grib2ValidationResult ValidateFile(string path, Grib2ValidationOptions options = null)
        {
            using var stream = File.OpenRead(path);
            return Validate(stream, options);
        }

        /// <summary>Valida un mensaje o colección de mensajes GRIB2 en memoria.</summary>
        public static Grib2ValidationResult Validate(byte[] data, Grib2ValidationOptions options = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return Run(new Grib2Reader(data), options);
        }

        /// <summary>Valida un flujo GRIB2. Debe ser legible y con posicionamiento.</summary>
        public static Grib2ValidationResult Validate(Stream stream, Grib2ValidationOptions options = null)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            return Run(new Grib2Reader(stream), options);
        }

        /// <summary>Atajo: verdadero si el fichero no presenta errores de integridad.</summary>
        public static bool IsValidFile(string path, Grib2ValidationOptions options = null)
            => ValidateFile(path, options).IsValid;

        /// <summary>Atajo: verdadero si los datos no presentan errores de integridad.</summary>
        public static bool IsValid(byte[] data, Grib2ValidationOptions options = null)
            => Validate(data, options).IsValid;

        private static Grib2ValidationResult Run(Grib2Reader r, Grib2ValidationOptions options)
        {
            options = options ?? Grib2ValidationOptions.Default;
            var result = new Grib2ValidationResult();
            try
            {
                ValidateCore(r, options, result);
            }
            catch (Exception ex)
            {
                // Red de seguridad: ninguna entrada debe provocar que la validación falle.
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.ReadFailure,
                    0, -1, SafePosition(r), $"Lectura interrumpida: {ex.Message}");
            }
            return result;
        }

        private static void ValidateCore(Grib2Reader r, Grib2ValidationOptions options, Grib2ValidationResult result)
        {
            long fileLength = r.Length;
            if (fileLength == 0)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.EmptyInput,
                    0, -1, 0, "La entrada está vacía.");
                return;
            }

            r.Position = 0;
            var peek = new byte[4];
            long lastMessageEnd = 0;
            int messageIndex = 0;

            while (r.Position + 4 <= fileLength && !result.IssueLimitReached)
            {
                if (r.PeekBytes(peek, 0, 4) < 4) break;

                if (!(peek[0] == 'G' && peek[1] == 'R' && peek[2] == 'I' && peek[3] == 'B'))
                {
                    r.Position++;
                    continue;
                }

                long messageStart = r.Position;
                if (messageStart > lastMessageEnd)
                {
                    Add(result, options, Grib2ValidationSeverity.Warning, Grib2ValidationCode.LeadingGarbage,
                        messageIndex + 1, -1, lastMessageEnd,
                        $"{messageStart - lastMessageEnd} byte(s) ajenos al formato antes del mensaje.");
                }

                messageIndex++;
                long consumed = ValidateMessage(r, options, result, messageIndex, messageStart, fileLength);
                lastMessageEnd = messageStart + consumed;
                r.Position = lastMessageEnd;
            }

            result.MessageCount = messageIndex;

            if (messageIndex == 0)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.NoMessageFound,
                    0, -1, 0, "No se encontró la marca 'GRIB': la entrada no es un fichero GRIB2.");
                return;
            }

            if (lastMessageEnd < fileLength && !result.IssueLimitReached)
            {
                Add(result, options, Grib2ValidationSeverity.Warning, Grib2ValidationCode.TrailingGarbage,
                    0, -1, lastMessageEnd,
                    $"{fileLength - lastMessageEnd} byte(s) sobrantes tras el último mensaje.");
            }
        }

        /// <summary>Valida un mensaje y devuelve cuántos bytes debe avanzar el escaneo.</summary>
        private static long ValidateMessage(Grib2Reader r, Grib2ValidationOptions options,
            Grib2ValidationResult result, int messageIndex, long messageStart, long fileLength)
        {
            const long IndicatorLength = 16;
            long available = fileLength - messageStart;

            if (available < IndicatorLength)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.UnexpectedEndOfFile,
                    messageIndex, 0, messageStart,
                    $"Sección 0 incompleta: requiere {IndicatorLength} bytes y sólo quedan {available}.");
                return available;
            }

            r.Position = messageStart + 7;
            int edition = r.ReadUInt8();
            long declared = (long)r.ReadUInt64();

            if (edition != 2)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidEdition,
                    messageIndex, 0, messageStart + 7,
                    $"Edición GRIB {edition}; esta librería sólo admite la edición 2.");
                return 4; // saltamos la marca y seguimos buscando mensajes
            }

            if (declared < IndicatorLength)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidMessageLength,
                    messageIndex, 0, messageStart + 8,
                    $"Longitud de mensaje declarada imposible: {declared} bytes.");
                return 4;
            }

            long messageEnd = messageStart + declared;
            if (declared > available)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.TruncatedMessage,
                    messageIndex, 0, messageStart,
                    $"Mensaje truncado: declara {declared} bytes y sólo hay {available} disponibles.");
                messageEnd = fileLength;
            }
            else
            {
                var tail = new byte[4];
                r.Position = messageEnd - 4;
                if (r.PeekBytes(tail, 0, 4) < 4 ||
                    !(tail[0] == '7' && tail[1] == '7' && tail[2] == '7' && tail[3] == '7'))
                {
                    Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.MissingEndMarker,
                        messageIndex, 8, messageEnd - 4,
                        "El mensaje no termina con el marcador '7777'.");
                }
            }

            ValidateSections(r, options, result, messageIndex, messageStart + IndicatorLength, messageEnd);
            return messageEnd - messageStart;
        }

        private static void ValidateSections(Grib2Reader r, Grib2ValidationOptions options,
            Grib2ValidationResult result, int messageIndex, long start, long messageEnd)
        {
            long pos = start;
            int previousNumber = 0;
            bool sawIdentification = false;
            bool sawData = false;

            // Estado del grupo en curso: en un mensaje las secciones 2-7 pueden
            // repetirse reutilizando las definiciones anteriores.
            int gridPointCount = -1;
            DataRepresentationSection drs = null;
            int drsPrecision = 0;      // sólo plantilla 5.4
            byte[] bitmap = null;
            bool hasBitmap = false;

            var peek = new byte[4];

            while (pos + 4 <= messageEnd && !result.IssueLimitReached)
            {
                r.Position = pos;
                if (r.PeekBytes(peek, 0, 4) < 4) break;
                if (peek[0] == '7' && peek[1] == '7' && peek[2] == '7' && peek[3] == '7')
                    break; // fin del mensaje

                if (pos + 5 > messageEnd)
                {
                    Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidSectionLength,
                        messageIndex, -1, pos, "Cabecera de sección incompleta antes del fin del mensaje.");
                    break;
                }

                r.Position = pos;
                var hdr = SectionHeader.Read(r);
                long sectionEnd = pos + hdr.Length;

                if (hdr.Length < 5)
                {
                    Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidSectionLength,
                        messageIndex, hdr.Number, pos,
                        $"Longitud de sección inválida: {hdr.Length} byte(s) (mínimo 5).");
                    break; // sin longitud fiable no se puede seguir recorriendo
                }

                if (sectionEnd > messageEnd)
                {
                    Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.SectionOutOfBounds,
                        messageIndex, hdr.Number, pos,
                        $"La sección {hdr.Number} declara {hdr.Length} byte(s) y se sale del mensaje en {sectionEnd - messageEnd} byte(s).");
                    break;
                }

                if (hdr.Number < 1 || hdr.Number > 7)
                {
                    Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.UnknownSectionNumber,
                        messageIndex, hdr.Number, pos + 4,
                        $"Número de sección {hdr.Number} fuera del rango 1-7.");
                    pos = sectionEnd;
                    previousNumber = hdr.Number;
                    continue;
                }

                // El formato exige orden ascendente; tras la sección 7 puede empezar
                // un nuevo grupo en la 2, 3 o 4.
                if (hdr.Number <= previousNumber && !(previousNumber == 7 && hdr.Number >= 2))
                {
                    Add(result, options, Grib2ValidationSeverity.Warning, Grib2ValidationCode.SectionOutOfOrder,
                        messageIndex, hdr.Number, pos + 4,
                        $"La sección {hdr.Number} aparece tras la sección {previousNumber}.");
                }

                switch (hdr.Number)
                {
                    case 1:
                        sawIdentification = true;
                        if (hdr.Length < 21)
                        {
                            Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidSectionLength,
                                messageIndex, 1, pos,
                                $"Sección 1 de {hdr.Length} byte(s); el formato exige al menos 21.");
                        }
                        break;

                    case 3:
                        gridPointCount = ValidateGridSection(r, options, result, messageIndex, pos, hdr.Length);
                        break;

                    case 5:
                        drs = ReadDataRepresentation(r, pos, hdr.Length, out drsPrecision);
                        if (drs == null)
                        {
                            Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidSectionLength,
                                messageIndex, 5, pos,
                                $"Sección 5 de {hdr.Length} byte(s); insuficiente para la plantilla de representación.");
                        }
                        else
                        {
                            ValidateDataRepresentation(options, result, messageIndex, pos, drs, gridPointCount);
                        }
                        break;

                    case 6:
                        ValidateBitmapSection(r, options, result, messageIndex, pos, hdr.Length,
                            gridPointCount, ref hasBitmap, ref bitmap);
                        break;

                    case 7:
                        sawData = true;
                        result.FieldCount++;
                        ValidateDataSection(r, options, result, messageIndex, pos, hdr.Length,
                            drs, drsPrecision, gridPointCount, hasBitmap, bitmap);
                        break;
                }

                previousNumber = hdr.Number;
                pos = sectionEnd;
            }

            if (result.IssueLimitReached) return;

            if (!sawIdentification)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.MissingSection,
                    messageIndex, 1, start, "El mensaje no contiene sección 1 (identificación).");
            }
            if (!sawData)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.MissingSection,
                    messageIndex, 7, start, "El mensaje no contiene sección 7 (datos).");
            }
        }

        /// <summary>Lee el número de puntos y la plantilla de la sección 3. Devuelve -1 si no se pudo leer.</summary>
        private static int ValidateGridSection(Grib2Reader r, Grib2ValidationOptions options,
            Grib2ValidationResult result, int messageIndex, long sectionStart, uint length)
        {
            if (length < 14)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidSectionLength,
                    messageIndex, 3, sectionStart,
                    $"Sección 3 de {length} byte(s); insuficiente para la cabecera de rejilla.");
                return -1;
            }

            r.Position = sectionStart + 5;  // octeto 6
            r.Skip(1);                      // 6: fuente de la definición
            long points = r.ReadUInt32();   // 7-10: número de puntos
            r.Skip(1);                      // 11: octetos de la lista opcional
            r.Skip(1);                      // 12: interpretación de la lista
            int template = r.ReadUInt16();  // 13-14

            if (points <= 0 || points > int.MaxValue)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.PointCountMismatch,
                    messageIndex, 3, sectionStart + 6,
                    $"Número de puntos de rejilla inválido: {points}.");
                return -1;
            }

            if (template != 0)
            {
                Add(result, options, Grib2ValidationSeverity.Warning, Grib2ValidationCode.UnsupportedTemplate,
                    messageIndex, 3, sectionStart + 12,
                    $"Plantilla de rejilla 3.{template} no soportada por la librería (sólo 3.0).");
                return (int)points;
            }

            // En la plantilla 3.0 el decodificador usa Ni×Nj: debe coincidir con
            // el recuento declarado en los octetos 7-10.
            if (length < 38) return (int)points;

            r.Position = sectionStart + 30;
            long ni = r.ReadUInt32();       // 31-34
            long nj = r.ReadUInt32();       // 35-38
            long product = ni * nj;

            if (product != points)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.PointCountMismatch,
                    messageIndex, 3, sectionStart + 30,
                    $"La rejilla declara {points} puntos pero Ni×Nj = {ni}×{nj} = {product}.");
            }

            return product > 0 && product <= int.MaxValue ? (int)product : (int)points;
        }

        /// <summary>
        /// Lee la sección 5 sin salirse de sus propios límites. Devuelve null si la
        /// sección es más corta de lo que exige su plantilla.
        /// </summary>
        private static DataRepresentationSection ReadDataRepresentation(
            Grib2Reader r, long sectionStart, uint length, out int precision)
        {
            precision = 0;
            if (length < 11) return null; // no cabe ni el número de valores y la plantilla

            r.Position = sectionStart + 5;
            int points = (int)r.ReadUInt32(); // 6-9
            int template = r.ReadUInt16();    // 10-11

            if (template == 4)
            {
                // 5.4 (IEEE) no comparte el bloque 12-21: sólo añade la precisión.
                if (length < 12) return null;
                precision = r.ReadUInt8();    // 12
                return new DataRepresentationSection
                {
                    DataPointCount = points,
                    Template = template,
                    RawTemplateBytes = new byte[0]
                };
            }

            // El resto de plantillas usadas en la práctica comparten los octetos 12-21.
            if (length < 21) return null;

            r.Position = sectionStart + 5;
            return DataRepresentationSection.Read(r, sectionStart, length);
        }

        private static void ValidateDataRepresentation(Grib2ValidationOptions options, Grib2ValidationResult result,
            int messageIndex, long sectionStart, DataRepresentationSection drs, int gridPointCount)
        {
            if (drs.DataPointCount < 0)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.PointCountMismatch,
                    messageIndex, 5, sectionStart + 5,
                    "El número de puntos de datos de la sección 5 desborda el rango admitido.");
                return;
            }

            if (gridPointCount >= 0 && drs.DataPointCount > gridPointCount)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.PointCountMismatch,
                    messageIndex, 5, sectionStart + 5,
                    $"La sección 5 declara {drs.DataPointCount} valores y la rejilla sólo tiene {gridPointCount} puntos.");
            }
        }

        private static void ValidateBitmapSection(Grib2Reader r, Grib2ValidationOptions options,
            Grib2ValidationResult result, int messageIndex, long sectionStart, uint length,
            int gridPointCount, ref bool hasBitmap, ref byte[] bitmap)
        {
            if (length < 6)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.InvalidSectionLength,
                    messageIndex, 6, sectionStart,
                    $"Sección 6 de {length} byte(s); falta el indicador de bitmap.");
                return;
            }

            r.Position = sectionStart + 5;
            int indicator = r.ReadUInt8();

            if (indicator == 255) // sin bitmap: todos los puntos presentes
            {
                hasBitmap = false;
                bitmap = new byte[0];
                return;
            }

            if (indicator == 254) // bitmap definido en un mensaje anterior
            {
                if (!hasBitmap)
                {
                    Add(result, options, Grib2ValidationSeverity.Warning, Grib2ValidationCode.MissingSection,
                        messageIndex, 6, sectionStart + 5,
                        "El indicador 254 remite a un bitmap previo que no aparece en la entrada.");
                }
                return; // conserva el bitmap anterior
            }

            if (indicator != 0)
            {
                Add(result, options, Grib2ValidationSeverity.Warning, Grib2ValidationCode.UnsupportedTemplate,
                    messageIndex, 6, sectionStart + 5,
                    $"Indicador de bitmap {indicator} reservado por el formato.");
            }

            hasBitmap = true;
            int bytes = (int)(sectionStart + length - r.Position);
            bitmap = bytes > 0 ? r.ReadBytes(bytes) : new byte[0];

            if (gridPointCount > 0)
            {
                int needed = (gridPointCount + 7) / 8;
                if (bitmap.Length < needed)
                {
                    Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.BitmapTooShort,
                        messageIndex, 6, sectionStart,
                        $"El bitmap ocupa {bitmap.Length} byte(s) y la rejilla de {gridPointCount} puntos requiere {needed}.");
                }
            }
        }

        private static void ValidateDataSection(Grib2Reader r, Grib2ValidationOptions options,
            Grib2ValidationResult result, int messageIndex, long sectionStart, uint length,
            DataRepresentationSection drs, int precision, int gridPointCount, bool hasBitmap, byte[] bitmap)
        {
            if (drs == null)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.MissingSection,
                    messageIndex, 7, sectionStart,
                    "Sección 7 sin una sección 5 previa que describa el empaquetado.");
                return;
            }

            int dataBytes = (int)(length - 5);

            // Sin bitmap, los valores empaquetados deben cubrir toda la rejilla.
            if (gridPointCount >= 0 && !hasBitmap && drs.DataPointCount != gridPointCount)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.PointCountMismatch,
                    messageIndex, 7, sectionStart,
                    $"Sin bitmap, la sección 5 declara {drs.DataPointCount} valores para una rejilla de {gridPointCount} puntos.");
            }

            // Con bitmap, los bits activos deben coincidir con los valores empaquetados.
            if (hasBitmap && bitmap != null && gridPointCount > 0 && bitmap.Length >= (gridPointCount + 7) / 8)
            {
                int present = CountSetBits(bitmap, gridPointCount);
                if (present != drs.DataPointCount)
                {
                    Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.PointCountMismatch,
                        messageIndex, 7, sectionStart,
                        $"El bitmap marca {present} punto(s) presentes y la sección 5 declara {drs.DataPointCount} valores.");
                }
            }

            long required = ExpectedDataBytes(drs, precision);
            if (required >= 0 && dataBytes < required)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.DataSectionTooShort,
                    messageIndex, 7, sectionStart,
                    $"La sección 7 aporta {dataBytes} byte(s) y la plantilla 5.{drs.Template} requiere {required}.");
            }

            IDataRepresentationDecoder decoder;
            try
            {
                decoder = Grib2Parser.SelectDecoder(drs.Template);
            }
            catch (GribNotSupportedException)
            {
                Add(result, options, Grib2ValidationSeverity.Warning, Grib2ValidationCode.UnsupportedTemplate,
                    messageIndex, 5, sectionStart,
                    $"Plantilla de representación 5.{drs.Template} no soportada por la librería.");
                return;
            }

            if (!options.DecodeData) return;

            try
            {
                r.Position = sectionStart + 5;
                var payload = dataBytes > 0 ? r.ReadBytes(dataBytes) : new byte[0];
                var decoded = decoder.Decode(drs, payload, drs.DataPointCount);
                if (gridPointCount > 0)
                {
                    BitmapApplier.Apply(decoded,
                        new BitmapSection { Indicator = hasBitmap ? 0 : 255, HasBitmap = hasBitmap, Bitmap = bitmap ?? new byte[0] },
                        gridPointCount);
                }
            }
            catch (Exception ex)
            {
                Add(result, options, Grib2ValidationSeverity.Error, Grib2ValidationCode.DecodeFailure,
                    messageIndex, 7, sectionStart,
                    $"Fallo al decodificar los datos ({ex.GetType().Name}): {ex.Message}");
            }
        }

        /// <summary>Bytes que exige el empaquetado, o -1 si la plantilla no permite calcularlo.</summary>
        private static long ExpectedDataBytes(DataRepresentationSection drs, int precision)
        {
            switch (drs.Template)
            {
                case 0: // simple packing
                    return ((long)drs.DataPointCount * drs.BitsPerValue + 7) / 8;
                case 4: // IEEE
                    int width = precision == 2 ? 8 : precision == 3 ? 16 : 4;
                    return (long)drs.DataPointCount * width;
                default:
                    // Complex packing (5.2/5.3), JPEG2000 (5.40)...: longitud variable.
                    return -1;
            }
        }

        private static int CountSetBits(byte[] bitmap, int pointCount)
        {
            int count = 0;
            int fullBytes = pointCount >> 3;
            for (int i = 0; i < fullBytes; i++)
            {
                int b = bitmap[i];
                while (b != 0) { count += b & 1; b >>= 1; }
            }
            int rest = pointCount & 7;
            if (rest > 0)
            {
                int b = bitmap[fullBytes];
                for (int k = 0; k < rest; k++)
                    count += (b >> (7 - k)) & 1;
            }
            return count;
        }

        private static void Add(Grib2ValidationResult result, Grib2ValidationOptions options,
            Grib2ValidationSeverity severity, Grib2ValidationCode code,
            int messageIndex, int sectionNumber, long offset, string message)
        {
            result.Add(severity, code, messageIndex, sectionNumber, offset, message);
            if (options.MaxIssues > 0 && result.Issues.Count >= options.MaxIssues)
                result.MarkLimitReached();
        }

        private static long SafePosition(Grib2Reader r)
        {
            try { return r.Position; }
            catch { return -1; }
        }
    }
}
