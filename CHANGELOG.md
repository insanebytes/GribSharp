# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]
### Added
- `RepresentedTime` field in Grib2Field to represent the date of the sample. ReferenceTime + ForecastHour.
- `CenterName` and `DisciplineName` in `Grib2Message`.
- `ReferenceTimeSignificance`,`ReferenceTimeSignificanceName` and `KnownReferenceTimeSignificance` in `Grib2Field` from Table 1.2

### Fixed
- CodeTable `Levels` and `LevelType` enum missing Table 4.5 levels.
- CodeTable `Centers` missing Table 0.

### Changed
- `Grib2Dumper` output now includes the new field `RepresentedTime`.
- `Grib2Dumper` output now includes the new fields `CenterName` and `DisciplineName`.
- `Grib2Dumper` output now includes the new fields `ReferenceTimeSignificanceName` and `ReferenceTimeSignificance`.
- 
## [1.0.12] - 2025-06-17
### Added
- Full support for GRIB2 files from NOAA NOMADS

### Changed
- `Grib2Dumper` output now includes discipline name, category name and product definition template.

### Fixed
- `GetValueAt` not interpolating correctly for coordinates between grid points

## [1.0.11] - 2025-06-17
### Added
- Stream-based parsing: `Grib2Reader` and `Grib2Parser` now accept `Stream` directly, supporting `FileStream` and `MemoryStream` without copying the entire buffer into memory.

### Changed
- `Grib2Reader` internally backed by `Stream` instead of `byte[]`. The `byte[]` constructor still works (wraps in `MemoryStream`).

## [1.0.0] - 2025-06-17

### Added
- Initial release.
- GRIB2 parsing with support for Simple Packing, Complex Packing, IEEE Float, and JPEG2000.
- Bitmap-based missing value handling.
- Grid definition and product metadata extraction.
