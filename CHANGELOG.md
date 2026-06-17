# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

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
