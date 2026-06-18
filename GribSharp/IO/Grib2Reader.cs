using System;
using System.IO;

namespace GribSharp.IO
{
    /// <summary>Lectura big-endian sobre un stream GRIB2.</summary>
    public sealed class Grib2Reader
    {
        private readonly Stream _stream;

        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public long Length => _stream.Length;

        public Grib2Reader(byte[] data, long offset = 0)
        {
            _stream = new MemoryStream(data ?? throw new ArgumentNullException(nameof(data)), false);
            if (offset != 0)
                _stream.Position = offset;
        }

        public Grib2Reader(Stream stream, long offset = 0)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable.", nameof(stream));
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable.", nameof(stream));
            if (offset != 0)
                _stream.Position = offset;
        }

        public byte ReadUInt8()
        {
            int b = _stream.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            return (byte)b;
        }

        public ushort ReadUInt16()
        {
            int b0 = _stream.ReadByte();
            int b1 = _stream.ReadByte();
            if (b1 < 0) throw new EndOfStreamException();
            return (ushort)((b0 << 8) | b1);
        }

        public uint ReadUInt24()
        {
            int b0 = _stream.ReadByte();
            int b1 = _stream.ReadByte();
            int b2 = _stream.ReadByte();
            if (b2 < 0) throw new EndOfStreamException();
            return (uint)((b0 << 16) | (b1 << 8) | b2);
        }

        public uint ReadUInt32()
        {
            int b0 = _stream.ReadByte();
            int b1 = _stream.ReadByte();
            int b2 = _stream.ReadByte();
            int b3 = _stream.ReadByte();
            if (b3 < 0) throw new EndOfStreamException();
            return (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
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
            ReadExact(bytes, 0, 4);
            byte tmp;
            tmp = bytes[0]; bytes[0] = bytes[3]; bytes[3] = tmp;
            tmp = bytes[1]; bytes[1] = bytes[2]; bytes[2] = tmp;
            return BitConverter.ToSingle(bytes, 0);
        }

        public byte[] ReadBytes(int n)
        {
            var buf = new byte[n];
            ReadExact(buf, 0, n);
            return buf;
        }

        public string ReadAscii(int n)
        {
            var buf = ReadBytes(n);
            return System.Text.Encoding.ASCII.GetString(buf, 0, n);
        }

        /// <summary>
        /// Skips n octets or bytes
        /// </summary>
        /// <param name="n"></param>
        public void Skip(int n) => _stream.Position += n;

        /// <summary>Lee bytes en la posición actual sin avanzar. Retorna cantidad efectivamente leída.</summary>
        public int PeekBytes(byte[] buffer, int offset, int count)
        {
            long saved = _stream.Position;
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = _stream.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0) break;
                totalRead += read;
            }
            _stream.Position = saved;
            return totalRead;
        }

        private void ReadExact(byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = _stream.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0) throw new EndOfStreamException();
                totalRead += read;
            }
        }
    }
}