# GribSharp

This project uses **CSJ2K**, which is distributed under the BSD 2-Clause License.

Copyright (c) 1999-2000 JJ2000 Partners; original C# port (c) 2007-2012 Jason S. Clary; C# encoding and adaptation to Portable Class Library with platform specific support (c) 2013-2016 Anders Gustafsson, Cureos AB

CSJ2K License: https://opensource.org/license/BSD-2-Clause

---

**GribSharp** is a pure C# library for reading and decoding **GRIB2 (GRIdded Binary Edition 2)** files used in meteorology, oceanography, and atmospheric modeling.

The library implements the core GRIB2 data representation templates and decoding algorithms in managed code, providing a lightweight and portable solution for .NET applications.

## Features

* Pure C# implementation only one dependency (CSJ2K for JPEG2000), compatible with Unity.
* Compatible with modern .NET runtimes.
* Reading of GRIB2 messages, sections, and metadata.
* Rich api for easy extraction.
* Direct api (more complex).

* Support for the following Data Representation Templates:

  * **Simple Packing**

    * Bit unpacking.
    * Value reconstruction using:
      `value = (R + X × 2^E) / 10^D`
  * **IEEE Floating Point**

    * Direct decoding of IEEE 32-bit floating-point values.
  * **Complex Packing**

    * Group references.
    * Variable bit widths.
    * Group-based decoding.
    * First- and second-order spatial differencing reconstruction.
  * **JPEG2000**

    * JPEG2000 codestream extraction.
    * Decoding through CSJ2K.
    * GRIB scaling and value reconstruction.
* **Bitmap Section** support:

  * Missing value handling.
  * Automatic conversion of undefined points to `float.NaN`.

* Dump Grib2 file to text


## Dependencies

GribSharp is implemented entirely in managed C# and has a single external dependency:

* **CSJ2K** — used exclusively for decoding JPEG2000-compressed GRIB2 fields.

## Goals

GribSharp aims to provide a lightweight, cross-platform, and easy-to-integrate GRIB2 decoding library for .NET applications without relying on native GRIB tooling.

## Install
This package is available in Nuget
```
dotnet add package GribSharp
```

## Usage
### Rich API:
```
using GribParser;
using GribParser.Model;

byte[] data = File.ReadAllBytes("gfs_forecast.grib2");
var file = Grib2Parser.ParseFile(data);

// List all available parameters
foreach (var name in file.ParameterNames)
    Console.WriteLine(name);

// Search by name (case-insensitive)
var temp = file.GetField("Temperature");
Console.WriteLine($"{temp.ParameterName} [{temp.Units}]: {temp.Values.Length} points");

// Search by enum (compile-time safe)
var rh = file[Parameter.RelativeHumidity];

// Search by level
var t2m = file.GetField(Parameter.Temperature, LevelType.HeightAboveGround, 2);
var t850 = file.GetField(Parameter.Temperature, LevelType.Isobaric, 85000);

// TryGet for optional parameters
if (file.TryGetField(Parameter.WindGust, out var gust))
    Console.WriteLine($"Racha máxima: {gust.Values.Max()} {gust.Units}");

// All fields of a parameter (ej: multiple level temperature)
var allTemps = file.GetFields(Parameter.Temperature);
foreach (var f in allTemps)
    Console.WriteLine($"  {f.LevelDescription} = {f.LevelValue}");

// Value at coordinates
float value = t2m.GetValueAt(lat: 40.41, lon: -3.70); // Madrid
```

---
### Direct API:
```
using GribParser;
using GribParser.Model;

byte[] data = File.ReadAllBytes("gfs_forecast.grib2");
var messages = Grib2Parser.Parse(data);

byte[] data = File.ReadAllBytes("gfs_forecast.grib2");
var messages = Grib2Parser.Parse(data);

// Iterate messages and fields
foreach (var msg in messages)
{
    foreach (var f in msg.Fields)
    {
        Console.WriteLine($"{f.ParameterName} [{f.Units}] @ {f.LevelDescription} {f.LevelValue}");
        Console.WriteLine($"  Grid: {f.Grid.Ni}x{f.Grid.Nj}, Forecast +{f.ForecastTime}h");
    }

    // Search field inside a message
    if (msg.TryGetField("Temperature", out var temp))
        Console.WriteLine($"  Temp min={temp.Values.Min():F1} max={temp.Values.Max():F1}");
}

// Debug: dump wgrib2 style
Console.WriteLine(Grib2Dumper.Dump(data));
```