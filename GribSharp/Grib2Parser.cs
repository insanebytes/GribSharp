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
        public static Grib2File ParseFile(string path)
        {
            using var stream = File.OpenRead(path);
            return new Grib2File(Parse(stream));
        }

        public static Grib2File ParseFile(Stream stream)
        {
            return new Grib2File(Parse(stream));
        }

        public static Grib2File ParseFile(byte[] data)
        {
            return new Grib2File(Parse(data));
        }

        public static List<Grib2Message> Parse(string path)
        {
            using var stream = File.OpenRead(path);
            return Parse(stream);
        }

        public static List<Grib2Message> Parse(Stream stream)
        {
            var r = new Grib2Reader(stream);
            return ParseCore(r);
        }

        public static List<Grib2Message> Parse(byte[] data)
        {
            var r = new Grib2Reader(data);
            return ParseCore(r);
        }

        private static List<Grib2Message> ParseCore(Grib2Reader r)
        {
            var messages = new List<Grib2Message>();
            var peek = new byte[4];

            while (r.Position < r.Length - 4)
            {
                if (r.PeekBytes(peek, 0, 4) < 4)
                    break;

                if (!(peek[0] == 'G' && peek[1] == 'R' && peek[2] == 'I' && peek[3] == 'B'))
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
                    if (r.PeekBytes(peek, 0, 4) >= 4 &&
                        peek[0] == '7' && peek[1] == '7' && peek[2] == '7' && peek[3] == '7')
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
                            r.Position = secStart + hdr.Length;
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
                DisciplineName = CodeTables.DisciplineDescription(discipline),
                ParameterCategory = pds.ParameterCategory,
                ParameterCategoryName = CodeTables.CategoryDescription(discipline, pds.ParameterCategory),
                ParameterNumber = pds.ParameterNumber,
                ParameterName = name,
                Units = units,
                ProductDefinitionTemplate = pds.Template,
                ProductDefinitionTemplateName = CodeTables.ProductDefinitionTemplateDescription(pds.Template),
                LevelType = pds.LevelType,
                LevelValue = pds.LevelValue,
                LevelDescription = CodeTables.LevelDescription(pds.LevelType),
                KnownDiscipline = CodeTables.ToDiscipline(discipline),
                KnownProductDefinitionTemplate = CodeTables.ToProductDefinitionTemplate(pds.Template),
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