using System;

namespace GribSharp.IO
{
    /// <summary>Lectura big-endian sobre un buffer GRIB2.</summary>
    public sealed class Grib2Reader
    {
        private readonly byte[] _data;
        public long Position { get; set; }
        public long Length => _data.Length;

        public Grib2Reader(byte[] data, long offset = 0)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            Position = offset;
        }

        public byte ReadUInt8() => _data[Position++];

        public ushort ReadUInt16()
        {
            int v = (_data[Position] << 8) | _data[Position + 1];
            Position += 2;
            return (ushort)v;
        }

        public uint ReadUInt24()
        {
            uint v = (uint)((_data[Position] << 16) | (_data[Position + 1] << 8) | _data[Position + 2]);
            Position += 3;
            return v;
        }

        public uint ReadUInt32()
        {
            uint v = (uint)((_data[Position] << 24) | (_data[Position + 1] << 16) |
                            (_data[Position + 2] << 8) | _data[Position + 3]);
            Position += 4;
            return v;
        }

        public ulong ReadUInt64()
        {
            ulong hi = ReadUInt32();
            ulong lo = ReadUInt32();
            return (hi << 32) | lo;
        }

        /// <summary>Entero signed-magnitude (bit más alto = signo), convención GRIB.</summary>
        public int ReadInt32Sm()
        {
            uint raw = ReadUInt32();
            int mag = (int)(raw & 0x7FFFFFFF);
            return (raw & 0x80000000) != 0 ? -mag : mag;
        }

        /// <summary>Entero de 16 bits signed-magnitude (convención GRIB).</summary>
        public short ReadInt16Sm()
        {
            ushort raw = ReadUInt16();
            int mag = raw & 0x7FFF;
            return (short)((raw & 0x8000) != 0 ? -mag : mag);
        }

        public float ReadFloat32()
        {
            var bytes = new byte[4];
            bytes[0] = _data[Position + 3];
            bytes[1] = _data[Position + 2];
            bytes[2] = _data[Position + 1];
            bytes[3] = _data[Position];
            Position += 4;
            return BitConverter.ToSingle(bytes, 0);
        }

        public byte[] ReadBytes(int n)
        {
            var outBuf = new byte[n];
            Array.Copy(_data, Position, outBuf, 0, n);
            Position += n;
            return outBuf;
        }

        public string ReadAscii(int n)
        {
            var s = System.Text.Encoding.ASCII.GetString(_data, (int)Position, n);
            Position += n;
            return s;
        }

        public void Skip(int n) => Position += n;
    }
}
