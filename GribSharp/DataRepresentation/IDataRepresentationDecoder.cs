using GribSharp.Sections;

namespace GribSharp.DataRepresentation
{
    /// <summary>sectionData = bytes de datos de la sección 7 (sin la cabecera de 5 bytes).</summary>
    public interface IDataRepresentationDecoder
    {
        float[] Decode(DataRepresentationSection drs, byte[] sectionData, int pointCount);
    }
}
