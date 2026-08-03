using System.Collections.Generic;
using System.Text;

namespace GribSharp.Validation
{
    /// <summary>Resultado de <see cref="Grib2Validator"/>: veredicto y lista de incidencias.</summary>
    public sealed class Grib2ValidationResult
    {
        private readonly List<Grib2ValidationIssue> _issues = new List<Grib2ValidationIssue>();
        private int _errorCount;
        private int _warningCount;

        /// <summary>Todas las incidencias en el orden en que se detectaron.</summary>
        public IReadOnlyList<Grib2ValidationIssue> Issues => _issues;

        /// <summary>Verdadero si no hay incidencias de gravedad <see cref="Grib2ValidationSeverity.Error"/>.</summary>
        public bool IsValid => _errorCount == 0;

        public int ErrorCount => _errorCount;

        public int WarningCount => _warningCount;

        /// <summary>Mensajes GRIB encontrados (incluidos los que dieron error).</summary>
        public int MessageCount { get; internal set; }

        /// <summary>Secciones de datos (campos) encontradas en todo el fichero.</summary>
        public int FieldCount { get; internal set; }

        /// <summary>
        /// Verdadero si se alcanzó <see cref="Grib2ValidationOptions.MaxIssues"/> y la
        /// validación se detuvo antes de recorrer todo el fichero.
        /// </summary>
        public bool IssueLimitReached { get; internal set; }

        public IEnumerable<Grib2ValidationIssue> Errors
        {
            get
            {
                foreach (var i in _issues)
                    if (i.Severity == Grib2ValidationSeverity.Error) yield return i;
            }
        }

        public IEnumerable<Grib2ValidationIssue> Warnings
        {
            get
            {
                foreach (var i in _issues)
                    if (i.Severity == Grib2ValidationSeverity.Warning) yield return i;
            }
        }

        internal void Add(Grib2ValidationSeverity severity, Grib2ValidationCode code,
            int messageIndex, int sectionNumber, long offset, string message)
        {
            if (IssueLimitReached) return;
            _issues.Add(new Grib2ValidationIssue(severity, code, messageIndex, sectionNumber, offset, message));
            if (severity == Grib2ValidationSeverity.Error) _errorCount++;
            else _warningCount++;
        }

        internal void MarkLimitReached() => IssueLimitReached = true;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(IsValid ? "VÁLIDO" : "INVÁLIDO");
            sb.Append(": ").Append(MessageCount).Append(" mensaje(s), ")
              .Append(FieldCount).Append(" campo(s), ")
              .Append(_errorCount).Append(" error(es), ")
              .Append(_warningCount).Append(" aviso(s).");
            if (IssueLimitReached) sb.Append(" (análisis detenido: límite de incidencias alcanzado)");
            foreach (var i in _issues)
                sb.AppendLine().Append("  ").Append(i);
            return sb.ToString();
        }
    }
}
