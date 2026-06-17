using System;

namespace GribSharp.Exceptions
{
    public sealed class GribNotSupportedException : Exception
    {
        public int Section { get; }
        public int Template { get; }
        public GribNotSupportedException(int section, int template)
            : base($"Plantilla no soportada: sección {section}, plantilla {template}.")
        {
            Section = section;
            Template = template;
        }
    }
}
