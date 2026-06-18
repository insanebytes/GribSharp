using System;
using System.Collections.Generic;

namespace GribSharp.Model
{
    public sealed class Grib2Message
    {
        public int Discipline;
        public string DisciplineName;
        public int Edition;
        public long Length;
        public int CenterId;
        public string CenterName;
        public List<Grib2Field> Fields = new List<Grib2Field>();

        public IReadOnlyList<string> FieldNames
        {
            get
            {
                var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var f in Fields) names.Add(f.ParameterName);
                return new List<string>(names);
            }
        }

        public Grib2Field GetField(string parameterName)
        {
            foreach (var f in Fields)
                if (string.Equals(f.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase))
                    return f;
            return null;
        }

        public IReadOnlyList<Grib2Field> GetFields(string parameterName)
        {
            var result = new List<Grib2Field>();
            foreach (var f in Fields)
                if (string.Equals(f.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase))
                    result.Add(f);
            return result;
        }

        public Grib2Field GetField(Parameter parameter)
        {
            foreach (var f in Fields)
                if (f.KnownParameter == parameter)
                    return f;
            return null;
        }

        public bool TryGetField(string parameterName, out Grib2Field field)
        {
            field = GetField(parameterName);
            return field != null;
        }

        public bool TryGetField(Parameter parameter, out Grib2Field field)
        {
            field = GetField(parameter);
            return field != null;
        }
    }
}
