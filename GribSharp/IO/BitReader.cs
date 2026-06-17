namespace GribSharp.IO
{
    /// <summary>Lee enteros de anchura de bit arbitraria, MSB-first.</summary>
    public sealed class BitReader
    {
        private readonly byte[] _data;
        private int _bytePos;
        private int _bitPos; // 0..7, 0 = bit más significativo del byte actual

        public BitReader(byte[] data, int startByte = 0)
        {
            _data = data;
            _bytePos = startByte;
            _bitPos = 0;
        }

        /// <summary>Avanza al siguiente límite de octeto si no está ya alineado.</summary>
        public void AlignToByte()
        {
            if (_bitPos != 0) { _bitPos = 0; _bytePos++; }
        }

        public long ReadBits(int count)
        {
            long result = 0;
            for (int i = 0; i < count; i++)
            {
                int bit = (_data[_bytePos] >> (7 - _bitPos)) & 1;
                result = (result << 1) | (uint)bit;
                _bitPos++;
                if (_bitPos == 8) { _bitPos = 0; _bytePos++; }
            }
            return result;
        }
    }
}
