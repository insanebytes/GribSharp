using GribSharp.Model;

var file = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", "gfs_sample.grib2");
var parsed = GribSharp.Grib2Parser.ParseFile(file);

foreach (var message in parsed.Messages)
{
    var field = message.GetField(Parameter.Temperature);
    string level = field.KnownLevelType?.ToString() ?? field.LevelDescription;
    var levelDistance = field.LevelValue;
    string units = field.Units;
    string name = field.ParameterName;

    //Ibiza airport
    //tested with xyGrib, must return 24.9Cº at 2meters field.KnownLevelType = HeightAboveGround field.LevelValue=2 
    var temperature = field.GetValueAt(38.8702778, 1.3702777777777777);
    var temperatureCelsius = temperature - 273.15;

    Console.WriteLine(
        $"Parameter: {name} Level: {level} Meters: {levelDistance} {temperatureCelsius}Cº File Units: {units}");
}