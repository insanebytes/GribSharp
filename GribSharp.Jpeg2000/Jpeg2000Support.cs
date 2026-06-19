using GribSharp.DataRepresentation;

namespace GribSharp.Jpeg2000
{
    /// <summary>
    /// Registro explícito del decodificador JPEG2000 (plantilla 5.40).
    /// El núcleo activa el add-on automáticamente mediante una sonda por reflexión,
    /// pero en escenarios con trimming/AOT donde la reflexión no es fiable puede
    /// llamarse a <see cref="Register"/> una vez al arrancar.
    /// </summary>
    public static class Jpeg2000Support
    {
        public static void Register()
        {
            DataRepresentationDecoderRegistry.Register(40, () => new Jpeg2000Decoder());
        }
    }
}
