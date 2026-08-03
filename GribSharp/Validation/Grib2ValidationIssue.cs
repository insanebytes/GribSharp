using System.Text;

namespace GribSharp.Validation
{
    /// <summary>Una anomalía concreta detectada por <see cref="Grib2Validator"/>.</summary>
    public sealed class Grib2ValidationIssue
    {
        public Grib2ValidationIssue(
            Grib2ValidationSeverity severity, Grib2ValidationCode code,
            int messageIndex, int sectionNumber, long offset, string message)
        {
            Severity = severity;
            Code = code;
            MessageIndex = messageIndex;
            SectionNumber = sectionNumber;
            Offset = offset;
            Message = message;
        }

        public Grib2ValidationSeverity Severity { get; }

        public Grib2ValidationCode Code { get; }

        /// <summary>Mensaje GRIB afectado (1-based), o 0 si la incidencia es del fichero completo.</summary>
        public int MessageIndex { get; }

        /// <summary>Sección GRIB2 afectada (0-7), o -1 si no aplica.</summary>
        public int SectionNumber { get; }

        /// <summary>Desplazamiento en bytes desde el inicio del fichero.</summary>
        public long Offset { get; }

        /// <summary>Descripción legible de la incidencia.</summary>
        public string Message { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(Severity).Append("] ").Append(Code);
            if (MessageIndex > 0) sb.Append(" msg=").Append(MessageIndex);
            if (SectionNumber >= 0) sb.Append(" sec=").Append(SectionNumber);
            sb.Append(" offset=").Append(Offset).Append(": ").Append(Message);
            return sb.ToString();
        }
    }
}
