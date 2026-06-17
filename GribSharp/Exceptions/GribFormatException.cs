using System;

namespace GribSharp.Exceptions
{
    public sealed class GribFormatException : Exception
    {
        public GribFormatException(string message) : base(message) { }
    }
}
