using System;
using System.Collections.Generic;

namespace GribSharp.Model
{
    public sealed class Grib2File
    {
        public IReadOnlyList<Grib2Message> Messages { get; }

        private readonly List<Grib2Field> _allFields;
        private readonly Dictionary<string, List<Grib2Field>> _byName;
        private readonly Dictionary<Parameter, List<Grib2Field>> _byParameter;

        public Grib2File(List<Grib2Message> messages)
        {
            Messages = messages;
            _allFields = new List<Grib2Field>();
            _byName = new Dictionary<string, List<Grib2Field>>(StringComparer.OrdinalIgnoreCase);
            _byParameter = new Dictionary<Parameter, List<Grib2Field>>();

            foreach (var msg in messages)
            {
                foreach (var f in msg.Fields)
                {
                    _allFields.Add(f);

                    if (!_byName.TryGetValue(f.ParameterName, out var nameList))
                    {
                        nameList = new List<Grib2Field>();
                        _byName[f.ParameterName] = nameList;
                    }
                    nameList.Add(f);

                    if (f.KnownParameter.HasValue)
                    {
                        if (!_byParameter.TryGetValue(f.KnownParameter.Value, out var paramList))
                        {
                            paramList = new List<Grib2Field>();
                            _byParameter[f.KnownParameter.Value] = paramList;
                        }
                        paramList.Add(f);
                    }
                }
            }
        }

        public IReadOnlyList<Grib2Field> Fields => _allFields;

        public IReadOnlyList<string> ParameterNames
        {
            get
            {
                var sorted = new List<string>(_byName.Keys);
                sorted.Sort(StringComparer.OrdinalIgnoreCase);
                return sorted;
            }
        }

        // --- Query by string name ---

        public Grib2Field GetField(string parameterName)
        {
            if (_byName.TryGetValue(parameterName, out var list) && list.Count > 0)
                return list[0];
            return null;
        }

        public IReadOnlyList<Grib2Field> GetFields(string parameterName)
        {
            if (_byName.TryGetValue(parameterName, out var list))
                return list;
            return Array.Empty<Grib2Field>();
        }

        public Grib2Field GetField(string parameterName, LevelType level, double levelValue)
        {
            if (!_byName.TryGetValue(parameterName, out var list))
                return null;
            foreach (var f in list)
                if (f.LevelType == (int)level && Math.Abs(f.LevelValue - levelValue) < 0.5)
                    return f;
            return null;
        }

        public bool TryGetField(string parameterName, out Grib2Field field)
        {
            field = GetField(parameterName);
            return field != null;
        }

        // --- Query by enum ---

        public Grib2Field GetField(Parameter parameter)
        {
            if (_byParameter.TryGetValue(parameter, out var list) && list.Count > 0)
                return list[0];
            return null;
        }

        public IReadOnlyList<Grib2Field> GetFields(Parameter parameter)
        {
            if (_byParameter.TryGetValue(parameter, out var list))
                return list;
            return Array.Empty<Grib2Field>();
        }

        public Grib2Field GetField(Parameter parameter, LevelType level, double levelValue)
        {
            if (!_byParameter.TryGetValue(parameter, out var list))
                return null;
            foreach (var f in list)
                if (f.LevelType == (int)level && Math.Abs(f.LevelValue - levelValue) < 0.5)
                    return f;
            return null;
        }

        public bool TryGetField(Parameter parameter, out Grib2Field field)
        {
            field = GetField(parameter);
            return field != null;
        }

        // --- Indexers ---

        public Grib2Field this[string parameterName] => GetField(parameterName);
        public Grib2Field this[Parameter parameter] => GetField(parameter);
    }
}
