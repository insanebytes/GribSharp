using System;
using GribSharp.IO;

namespace GribSharp.Sections
{
    /// <summary>Sección 1. sectionStart apunta al primer byte de la sección (octeto 1).</summary>
    public sealed class IdentificationSection
    {
        public int CenterId;
        public DateTime ReferenceTime;

        public static IdentificationSection Read(Grib2Reader r, long sectionStart, uint length)
        {
            // Tras SectionHeader.Read, r está en octeto 6 (offset 5).
            var s = new IdentificationSection { CenterId = r.ReadUInt16() };
            r.Skip(2);              // sub-centro
            r.Skip(1);              // tabla maestra
            r.Skip(1);              // tabla local
            r.Skip(1);              // significancia tiempo ref
            int year = r.ReadUInt16();
            int month = r.ReadUInt8();
            int day = r.ReadUInt8();
            int hour = r.ReadUInt8();
            int minute = r.ReadUInt8();
            int second = r.ReadUInt8();
            s.ReferenceTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            // Saltar al final de la sección por longitud (robusto ante campos extra).
            r.Position = sectionStart + length;
            return s;
        }
    }
}
