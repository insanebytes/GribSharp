using GribSharp.IO;

namespace GribSharp.Sections
{
    public sealed class BitmapSection
    {
        public int Indicator;
        public bool HasBitmap;
        public byte[] Bitmap;

        public static BitmapSection Read(Grib2Reader r, long sectionStart, uint length)
        {
            var s = new BitmapSection { Indicator = r.ReadUInt8() }; // octeto 6
            s.HasBitmap = s.Indicator != 255;
            if (s.HasBitmap)
            {
                int bytes = (int)(sectionStart + length - r.Position);
                s.Bitmap = r.ReadBytes(bytes);
            }
            else
            {
                s.Bitmap = new byte[0];
            }
            r.Position = sectionStart + length;
            return s;
        }
    }

    public static class BitmapApplier
    {
        public static float[] Apply(float[] decoded, BitmapSection bmp, int totalPoints)
        {
            if (!bmp.HasBitmap) return decoded;
            var full = new float[totalPoints];
            int di = 0;
            for (int i = 0; i < totalPoints; i++)
            {
                int bit = (bmp.Bitmap[i >> 3] >> (7 - (i & 7))) & 1;
                full[i] = bit == 1 ? decoded[di++] : float.NaN;
            }
            return full;
        }
    }
}
