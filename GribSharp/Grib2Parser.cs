using System.Collections.Generic;
using System.IO;
using GribSharp.DataRepresentation;
using GribSharp.Exceptions;
using GribSharp.IO;
using GribSharp.Model;
using GribSharp.Sections;
using GribSharp.Tables;

namespace GribSharp
{
    public static class Grib2Parser
    {
        public static List<Grib2Message> Parse(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return Parse(ms.ToArray());
            }
        }

        public static Grib2File ParseFile(Stream stream)
        {
            return new Grib2File(Parse(stream));
        }

        public static Grib2File ParseFile(byte[] data)
        {
            return new Grib2File(Parse(data));
        }

        public static List<Grib2Message> Parse(byte[] data)
        {
            var messages = new List<Grib2Message>();
            var r = new Grib2Reader(data);

            while (r.Position < r.Length - 4)
            {
                // Buscar "GRIB".
                if (!(data[r.Position] == 'G' && data[r.Position + 1] == 'R' &&
                      data[r.Position + 2] == 'I' && data[r.Position + 3] == 'B'))
                {
                    r.Position++;
                    continue;
                }

                long messageStart = r.Position;
                var ind = IndicatorSection.Read(r);
                var message = new Grib2Message
                {
                    Discipline = ind.Discipline,
                    Edition = ind.Edition,
                    Length = ind.TotalLength
                };

                IdentificationSection ids = null;
                GridDefinitionSection gds = null;
                ProductDefinitionSection pds = null;
                DataRepresentationSection drs = null;
                BitmapSection bmp = null;

                while (r.Position < messageStart + ind.TotalLength)
                {
                    // ¿Fin "7777"?
                    if (data[r.Position] == '7' && data[r.Position + 1] == '7' &&
                        data[r.Position + 2] == '7' && data[r.Position + 3] == '7')
                    {
                        r.Position += 4;
                        break;
                    }

                    long secStart = r.Position;
                    var hdr = SectionHeader.Read(r);
                    switch (hdr.Number)
                    {
                        case 1:
                            ids = IdentificationSection.Read(r, secStart, hdr.Length);
                            message.CenterId = ids.CenterId;
                            break;
                        case 2:
                            r.Position = secStart + hdr.Length; // local, ignorar
                            break;
                        case 3:
                            gds = GridDefinitionSection.Read(r, secStart, hdr.Length);
                            break;
                        case 4:
                            pds = ProductDefinitionSection.Read(r, secStart, hdr.Length);
                            break;
                        case 5:
                            drs = DataRepresentationSection.Read(r, secStart, hdr.Length);
                            break;
                        case 6:
                            bmp = BitmapSection.Read(r, secStart, hdr.Length);
                            break;
                        case 7:
                            var ds = DataSection.Read(r, secStart, hdr.Length);
                            message.Fields.Add(BuildField(message.Discipline, ids, gds, pds, drs, bmp, ds));
                            break;
                        default:
                            r.Position = secStart + hdr.Length;
                            break;
                    }
                }

                messages.Add(message);
                r.Position = messageStart + ind.TotalLength;
            }

            return messages;
        }

        private static Grib2Field BuildField(
            int discipline, IdentificationSection ids, GridDefinitionSection gds,
            ProductDefinitionSection pds, DataRepresentationSection drs, BitmapSection bmp, DataSection ds)
        {
            int totalPoints = gds.Grid.PointCount;
            var decoder = SelectDecoder(drs.Template);
            float[] decoded = decoder.Decode(drs, ds.Data, drs.DataPointCount);
            float[] values = BitmapApplier.Apply(decoded, bmp ?? new BitmapSection { Indicator = 255 }, totalPoints);

            var (name, units) = CodeTables.Parameter(discipline, pds.ParameterCategory, pds.ParameterNumber);

            return new Grib2Field
            {
                Discipline = discipline,
                ParameterCategory = pds.ParameterCategory,
                ParameterNumber = pds.ParameterNumber,
                ParameterName = name,
                Units = units,
                LevelType = pds.LevelType,
                LevelValue = pds.LevelValue,
                LevelDescription = CodeTables.LevelDescription(pds.LevelType),
                KnownParameter = CodeTables.ToParameter(discipline, pds.ParameterCategory, pds.ParameterNumber),
                KnownLevelType = CodeTables.ToLevelType(pds.LevelType),
                ReferenceTime = ids?.ReferenceTime ?? default,
                ForecastTime = pds.ForecastTime,
                Grid = gds.Grid,
                Values = values
            };
        }

        private static IDataRepresentationDecoder SelectDecoder(int template)
        {
            switch (template)
            {
                case 0: return new SimplePackingDecoder();
                case 2:
                case 3: return new ComplexPackingDecoder();
                case 4: return new IeeeFloatDecoder();
                case 40: return new Jpeg2000Decoder();
                default: throw new GribNotSupportedException(5, template);
            }
        }
    }
}
