namespace GribSharp.Model
{
    public enum DataRepresentationTemplate
    {
        SimplePacking = 0,
        ComplexPacking = 2,
        ComplexPackingSpatialDiff = 3,
        IeeeFloat = 4,
        Jpeg2000 = 40
    }

    public enum Discipline
    {
        Meteorological = 0,
        Hydrological = 1,
        LandSurface = 2,
        Space = 3,
        Oceanographic = 10
    }

    public enum LevelType
    {
        GroundOrWaterSurface = 1,
        CloudBase = 2,
        CloudTop = 3,
        ZeroDegCIsotherm = 4,
        MaximumWindLevel = 6,
        Tropopause = 7,
        Isobaric = 100,
        MeanSeaLevel = 101,
        SpecificAltitudeAboveMSL = 102,
        HeightAboveGround = 103,
        SigmaLevel = 104,
        HybridLevel = 105,
        DepthBelowLandSurface = 106,
        PressureDifferenceFromGround = 108,
        PotentialVorticitySurface = 109,
        EntireAtmosphere = 200,
        PlanetaryBoundaryLayer = 220
    }

    /// <summary>
    /// Known GRIB2 parameters. Composite encoding: disc * 1_000_000 + cat * 1_000 + num.
    /// </summary>
    public enum Parameter
    {
        // Discipline 0, Category 0: Temperature
        Temperature = 0_000_000,
        VirtualTemperature = 0_000_001,
        PotentialTemperature = 0_000_002,
        MaximumTemperature = 0_000_004,
        MinimumTemperature = 0_000_005,
        DewpointTemperature = 0_000_006,
        DewpointDepression = 0_000_007,
        TemperatureAnomaly = 0_000_009,
        LatentHeatNetFlux = 0_000_010,
        SensibleHeatNetFlux = 0_000_011,
        ApparentTemperature = 0_000_021,

        // Discipline 0, Category 1: Moisture
        SpecificHumidity = 0_001_000,
        RelativeHumidity = 0_001_001,
        HumidityMixingRatio = 0_001_002,
        PrecipitableWater = 0_001_003,
        PrecipitationRate = 0_001_007,
        TotalPrecipitation = 0_001_008,
        LargeScalePrecipitation = 0_001_009,
        ConvectivePrecipitation = 0_001_010,
        SnowDepth = 0_001_011,
        SnowWaterEquivalent = 0_001_013,
        CloudMixingRatio = 0_001_022,
        TotalSnowfall = 0_001_029,
        PercentFrozenPrecipitation = 0_001_039,
        TotalWaterPrecipitation = 0_001_049,

        // Discipline 0, Category 2: Momentum (Wind)
        WindDirection = 0_002_000,
        WindSpeed = 0_002_001,
        UWindComponent = 0_002_002,
        VWindComponent = 0_002_003,
        VerticalVelocityPressure = 0_002_008,
        VerticalVelocityGeometric = 0_002_009,
        AbsoluteVorticity = 0_002_010,
        PotentialVorticity = 0_002_014,
        WindGust = 0_002_022,

        // Discipline 0, Category 3: Mass (Pressure)
        Pressure = 0_003_000,
        PressureReducedToMSL = 0_003_001,
        PressureTendency = 0_003_002,
        ICAOStandardAtmosphereHeight = 0_003_003,
        Geopotential = 0_003_004,
        GeopotentialHeight = 0_003_005,
        GeometricHeight = 0_003_006,
        GeopotentialHeightAnomaly = 0_003_009,
        Density = 0_003_010,
        AltimeterSetting = 0_003_011,

        // Discipline 0, Category 4: Short-wave Radiation
        DownwardShortWaveRadiationFlux = 0_004_007,
        UpwardShortWaveRadiationFlux = 0_004_008,
        NetShortWaveRadiationFlux = 0_004_009,

        // Discipline 0, Category 5: Long-wave Radiation
        DownwardLongWaveRadiationFlux = 0_005_003,
        UpwardLongWaveRadiationFlux = 0_005_004,
        NetLongWaveRadiationFlux = 0_005_005,

        // Discipline 0, Category 6: Cloud
        TotalCloudCover = 0_006_001,
        LowCloudCover = 0_006_003,
        MediumCloudCover = 0_006_004,
        HighCloudCover = 0_006_005,
        CloudWater = 0_006_006,
        CloudCover = 0_006_022,

        // Discipline 0, Category 7: Thermodynamic Stability
        CAPE = 0_007_006,
        ConvectiveInhibition = 0_007_007,
        StormRelativeHelicity = 0_007_008,

        // Discipline 0, Category 19: Physical Properties
        Visibility = 0_019_000,
        Albedo = 0_019_001,

        // Discipline 2, Category 0: Land Surface
        LandCover = 2_000_000,
        SurfaceRoughness = 2_000_001,
        SoilTemperature = 2_000_002,
        SoilMoistureContent = 2_000_003,
        WaterRunoff = 2_000_005,
        VolumetricSoilMoisture = 2_000_022,

        // Discipline 10: Oceanography
        IceCover = 10_002_000,
        IceThickness = 10_002_001,
        WaterTemperature = 10_003_000
    }
}
