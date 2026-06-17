using System;

namespace GribSharp.Tests
{
    /// <summary>Construye mensajes GRIB2 sintéticos para pruebas de extremo a extremo.</summary>
    internal static class SyntheticGrib
    {
        public static byte[] BuildSimplePacked2x2(float[] values)
        {
            var sec1body = new byte[16]; // cuerpo desde octeto 6 (21 - 5)
            // centro 7 (octetos 6-7 => índice 0-1), fecha 2024-06-17 (octetos 13.. => índice 7..)
            sec1body[0] = 0; sec1body[1] = 7;
            sec1body[7] = (byte)(2024 >> 8); sec1body[8] = 2024 & 0xFF;
            sec1body[9] = 6; sec1body[10] = 17;
            var sec1 = Section(1, sec1body);

            var t3 = new byte[67]; // cuerpo sección 3 desde octeto 6 (índice = octeto - 6)
            PutU16(t3, 7, 0);                 // plantilla 3.0 (octetos 13-14)
            PutU32(t3, 25, 2);                // Ni (octetos 31-34)
            PutU32(t3, 29, 2);                // Nj (octetos 35-38)
            PutI32(t3, 41, 1_000000);         // La1 (octetos 47-50)
            PutI32(t3, 45, 0);                // Lo1 (octetos 51-54)
            PutI32(t3, 50, 0);                // La2 (octetos 56-59)
            PutI32(t3, 54, 1_000000);         // Lo2 (octetos 60-63)
            PutU32(t3, 58, 1_000000);         // Di (octetos 64-67)
            PutU32(t3, 62, 1_000000);         // Dj (octetos 68-71)
            t3[66] = 0;                       // scan mode (octeto 72)
            var sec3 = Section(3, t3);

            var t4 = new byte[23]; // cuerpo sección 4 desde octeto 6
            PutU16(t4, 0, 0);  // NV
            PutU16(t4, 2, 0);  // plantilla 4.0
            t4[4] = 0;         // categoría (octeto 10)
            t4[5] = 0;         // número (octeto 11)
            t4[12] = 1;        // unidad rango tiempo (octeto 18)
            PutU32(t4, 13, 0); // forecast time (octetos 19-22)
            t4[17] = 1;        // tipo superficie (octeto 23)
            t4[18] = 0;        // factor escala (octeto 24)
            PutI32(t4, 19, 0); // valor escalado (octetos 25-28)
            var sec4 = Section(4, t4);

            var t5 = new byte[16]; // cuerpo sección 5 desde octeto 6
            PutU32(t5, 0, (uint)values.Length); // número de puntos (octetos 6-9)
            PutU16(t5, 4, 0);                    // plantilla 5.0 (octetos 10-11)
            // R=0 (octetos 12-15), E=0, D=0
            t5[14] = 8;                          // bits por valor (octeto 20)
            t5[15] = 0;                          // tipo original (octeto 21)
            var sec5 = Section(5, t5);

            var sec6body = new byte[1]; sec6body[0] = 255; // sin bitmap
            var sec6 = Section(6, sec6body);

            var sec7body = new byte[values.Length];
            for (int i = 0; i < values.Length; i++) sec7body[i] = (byte)values[i];
            var sec7 = Section(7, sec7body);

            // Sección 0
            long total = 16 + sec1.Length + sec3.Length + sec4.Length + sec5.Length + sec6.Length + sec7.Length + 4;
            var sec0 = new byte[16];
            sec0[0] = (byte)'G'; sec0[1] = (byte)'R'; sec0[2] = (byte)'I'; sec0[3] = (byte)'B';
            sec0[6] = 0; sec0[7] = 2;
            for (int i = 0; i < 8; i++) sec0[15 - i] = (byte)((total >> (8 * i)) & 0xFF);

            var end = new byte[] { (byte)'7', (byte)'7', (byte)'7', (byte)'7' };

            return Concat(sec0, sec1, sec3, sec4, sec5, sec6, sec7, end);
        }

        // Envuelve el cuerpo (desde octeto 6) en una sección con length(4)+number(1).
        private static byte[] Section(int number, byte[] bodyFromOctet6)
        {
            int len = 5 + bodyFromOctet6.Length;
            var b = new byte[len];
            b[0] = (byte)(len >> 24); b[1] = (byte)(len >> 16); b[2] = (byte)(len >> 8); b[3] = (byte)len;
            b[4] = (byte)number;
            Array.Copy(bodyFromOctet6, 0, b, 5, bodyFromOctet6.Length);
            return b;
        }

        private static void PutU16(byte[] b, int o, int v) { b[o] = (byte)(v >> 8); b[o + 1] = (byte)v; }
        private static void PutU32(byte[] b, int o, uint v)
        { b[o] = (byte)(v >> 24); b[o + 1] = (byte)(v >> 16); b[o + 2] = (byte)(v >> 8); b[o + 3] = (byte)v; }
        private static void PutI32(byte[] b, int o, int v)
        { uint raw = v < 0 ? (0x80000000u | (uint)(-v)) : (uint)v; PutU32(b, o, raw); }

        private static byte[] Concat(params byte[][] parts)
        {
            int n = 0; foreach (var p in parts) n += p.Length;
            var outb = new byte[n]; int k = 0;
            foreach (var p in parts) { Array.Copy(p, 0, outb, k, p.Length); k += p.Length; }
            return outb;
        }
    }
}
