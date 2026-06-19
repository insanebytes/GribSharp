using System;
using System.Collections.Generic;

namespace GribSharp.DataRepresentation
{
    /// <summary>
    /// Registro de decodificadores de plantillas de representación de datos
    /// (sección 5) no incluidas en el núcleo. Permite que paquetes add-on
    /// (p. ej. <c>GribSharp.Jpeg2000</c>) aporten soporte para plantillas como la
    /// 5.40 (JPEG2000) sin que el núcleo dependa de librerías externas.
    /// </summary>
    public static class DataRepresentationDecoderRegistry
    {
        private static readonly Dictionary<int, Func<IDataRepresentationDecoder>> _factories
            = new Dictionary<int, Func<IDataRepresentationDecoder>>();
        private static readonly object _gate = new object();

        /// <summary>Registra una fábrica de decodificador para una plantilla.</summary>
        public static void Register(int template, Func<IDataRepresentationDecoder> factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            lock (_gate) _factories[template] = factory;
        }

        /// <summary>Crea un decodificador para la plantilla si hay fábrica registrada.</summary>
        public static bool TryCreate(int template, out IDataRepresentationDecoder decoder)
        {
            Func<IDataRepresentationDecoder> factory;
            lock (_gate)
            {
                if (!_factories.TryGetValue(template, out factory))
                {
                    decoder = null;
                    return false;
                }
            }
            decoder = factory();
            return true;
        }
    }
}
