// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Heifs;
using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Tests.Unit.Heifs;

public class HeifExamplesTests
{
	[Fact]
	public void CreateWebOptimized_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateWebOptimized(1920, 1080, true);

		// Assert
		Assert.Equal(1920, heif.Width);
		Assert.Equal(1080, heif.Height);
		Assert.True(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.Web, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Fast, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(8, heif.BitDepth);
		Assert.Equal(0, heif.ThreadCount);
		Assert.True(heif.GenerateThumbnails);
		Assert.True(heif.EnableProgressiveDecoding);
		Assert.Equal("Wangkanai Graphics Library", heif.Metadata.Software);
		Assert.NotNull(heif.Metadata.CreationTime);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateWebOptimized_WithoutAlpha_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateWebOptimized(1280, 720);

		// Assert
		Assert.Equal(1280, heif.Width);
		Assert.Equal(720, heif.Height);
		Assert.False(heif.HasAlpha);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateHighQuality_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateHighQuality(3840, 2160, true);

		// Assert
		Assert.Equal(3840, heif.Width);
		Assert.Equal(2160, heif.Height);
		Assert.True(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.Professional, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Slow, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv444, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(10, heif.BitDepth);
		Assert.Equal(HeifProfile.Main10, heif.Profile);
		Assert.Equal(0, heif.ThreadCount);
		Assert.True(heif.GenerateThumbnails);
		Assert.False(heif.EnableProgressiveDecoding);
		Assert.Equal("Wangkanai Graphics Library - Professional", heif.Metadata.Software);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateLossless_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateLossless(2560, 1440, false);

		// Assert
		Assert.Equal(2560, heif.Width);
		Assert.Equal(1440, heif.Height);
		Assert.False(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.Lossless, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Slowest, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv444, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(12, heif.BitDepth);
		Assert.Equal(HeifProfile.Main10, heif.Profile);
		Assert.True(heif.IsLossless);
		Assert.Equal(0, heif.ThreadCount);
		Assert.True(heif.GenerateThumbnails);
		Assert.False(heif.EnableProgressiveDecoding);
		Assert.Equal("Wangkanai Graphics Library - Lossless", heif.Metadata.Software);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateFast_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateFast(800, 600, true);

		// Assert
		Assert.Equal(800, heif.Width);
		Assert.Equal(600, heif.Height);
		Assert.True(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.Standard, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Fastest, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(8, heif.BitDepth);
		Assert.Equal(Environment.ProcessorCount, heif.ThreadCount);
		Assert.False(heif.GenerateThumbnails);
		Assert.False(heif.EnableProgressiveDecoding);
		Assert.Equal("Wangkanai Graphics Library - Fast", heif.Metadata.Software);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateHdr_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateHdr(3840, 2160, false);

		// Assert
		Assert.Equal(3840, heif.Width);
		Assert.Equal(2160, heif.Height);
		Assert.False(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.High, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Medium, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv444, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(HeifColorSpace.Bt2100Pq, heif.ColorSpace);
		Assert.Equal(10, heif.BitDepth);
		Assert.Equal(HeifProfile.Main10, heif.Profile);
		Assert.Equal(0, heif.ThreadCount);
		Assert.True(heif.GenerateThumbnails);
		Assert.True(heif.EnableProgressiveDecoding);
		Assert.True(heif.HasHdrMetadata);
		Assert.Equal("Wangkanai Graphics Library - HDR", heif.Metadata.Software);
		Assert.Equal("BT.2100 PQ HDR", heif.Metadata.ColorSpaceInfo);
		Assert.True(heif.IsValid());

		// Verify HDR metadata
		var hdrMetadata = heif.Metadata.HdrMetadata;
		Assert.NotNull(hdrMetadata);
		Assert.Equal(HeifConstants.Hdr.Hdr10PeakBrightness, hdrMetadata.MaxLuminance);
		Assert.Equal(0.005, hdrMetadata.MinLuminance);
		Assert.Equal(1000, hdrMetadata.MaxContentLightLevel);
		Assert.Equal(400, hdrMetadata.MaxFrameAverageLightLevel);
		Assert.Equal(HdrColorPrimaries.Bt2020, hdrMetadata.ColorPrimaries);
		Assert.Equal(HdrTransferCharacteristics.Pq, hdrMetadata.TransferCharacteristics);
		Assert.Equal(HdrMatrixCoefficients.Bt2020Ncl, hdrMetadata.MatrixCoefficients);
	}

	[Fact]
	public void CreateMobile_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateMobile(1080, 1920, true);

		// Assert
		Assert.Equal(1080, heif.Width);
		Assert.Equal(1920, heif.Height);
		Assert.True(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.Mobile, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.MediumFast, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(8, heif.BitDepth);
		Assert.True(heif.ThreadCount <= 4);
		Assert.True(heif.ThreadCount <= Environment.ProcessorCount);
		Assert.True(heif.GenerateThumbnails);
		Assert.True(heif.EnableProgressiveDecoding);
		Assert.Equal("Wangkanai Graphics Library - Mobile", heif.Metadata.Software);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateThumbnail_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateThumbnail(256, 256);

		// Assert
		Assert.Equal(256, heif.Width);
		Assert.Equal(256, heif.Height);
		Assert.False(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.Thumbnail, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Fastest, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(8, heif.BitDepth);
		Assert.Equal(1, heif.ThreadCount);
		Assert.False(heif.GenerateThumbnails);
		Assert.False(heif.EnableProgressiveDecoding);
		Assert.Equal("Wangkanai Graphics Library - Thumbnail", heif.Metadata.Software);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateThumbnail_WithLargeDimensions_ClampsToDimensions()
	{
		// Act
		var heif = HeifExamples.CreateThumbnail(1024, 768);

		// Assert
		Assert.Equal(512, heif.Width); // Clamped to 512
		Assert.Equal(512, heif.Height); // Clamped to 512
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateAv1_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateAv1(1920, 1080, true);

		// Assert
		Assert.Equal(1920, heif.Width);
		Assert.Equal(1080, heif.Height);
		Assert.True(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.NearLossless, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.MediumSlow, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Av1, heif.Compression);
		Assert.Equal(10, heif.BitDepth);
		Assert.Equal(0, heif.ThreadCount);
		Assert.True(heif.GenerateThumbnails);
		Assert.True(heif.EnableProgressiveDecoding);
		Assert.Equal("Wangkanai Graphics Library - AV1", heif.Metadata.Software);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateDemo_WithDefaultDimensions_ReturnsCompleteExample()
	{
		// Act
		var heif = HeifExamples.CreateDemo();

		// Assert
		Assert.Equal(1920, heif.Width);
		Assert.Equal(1080, heif.Height);
		Assert.False(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.High, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Default, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(8, heif.BitDepth);
		Assert.Equal(0, heif.ThreadCount);
		Assert.True(heif.GenerateThumbnails);
		Assert.True(heif.EnableProgressiveDecoding);
		Assert.True(heif.IsValid());

		// Verify comprehensive metadata
		Assert.Equal("Wangkanai Graphics Library - Demo", heif.Metadata.Software);
		Assert.Equal("Demonstration HEIF image created with Wangkanai Graphics Library", heif.Metadata.Description);
		Assert.Equal("© 2025 Wangkanai", heif.Metadata.Copyright);
		Assert.Equal("Graphics Library", heif.Metadata.Author);
		Assert.NotNull(heif.Metadata.CameraMetadata);
		Assert.Equal("Demo Camera", heif.Metadata.CameraMetadata.CameraMake);
		Assert.Equal("Model X", heif.Metadata.CameraMetadata.CameraModel);
		Assert.Equal("Demo Lens", heif.Metadata.CameraMetadata.LensMake);
		Assert.Equal("50mm f/1.8", heif.Metadata.CameraMetadata.LensModel);
		Assert.Equal(50.0, heif.Metadata.CameraMetadata.FocalLength);
		Assert.Equal(1.8, heif.Metadata.CameraMetadata.Aperture);
		Assert.Equal(1.0 / 60.0, heif.Metadata.CameraMetadata.ExposureTime);
		Assert.Equal(100, heif.Metadata.CameraMetadata.IsoSensitivity);
		Assert.Equal(300.0, heif.Metadata.CameraMetadata.XResolution);
		Assert.Equal(300.0, heif.Metadata.CameraMetadata.YResolution);
		Assert.Equal("sRGB", heif.Metadata.ColorSpaceInfo);
		Assert.Equal("Auto", heif.Metadata.WhiteBalance);
		Assert.NotNull(heif.Metadata.CreationTime);

		// Verify GPS coordinates (Sydney Opera House)
		Assert.NotNull(heif.Metadata.GpsCoordinates);
		Assert.Equal(-33.8568, heif.Metadata.GpsCoordinates.Latitude);
		Assert.Equal(151.2153, heif.Metadata.GpsCoordinates.Longitude);
		Assert.Equal(5.0, heif.Metadata.GpsCoordinates.Altitude);
	}

	[Fact]
	public void CreateDemo_WithCustomDimensions_ReturnsCompleteExample()
	{
		// Act
		var heif = HeifExamples.CreateDemo(2560, 1440);

		// Assert
		Assert.Equal(2560, heif.Width);
		Assert.Equal(1440, heif.Height);
		Assert.True(heif.IsValid());
	}

	[Fact]
	public void CreateProfessionalPhoto_WithValidDimensions_ReturnsConfiguredRaster()
	{
		// Act
		var heif = HeifExamples.CreateProfessionalPhoto(3840, 2160);

		// Assert
		Assert.Equal(3840, heif.Width);
		Assert.Equal(2160, heif.Height);
		Assert.False(heif.HasAlpha);
		Assert.Equal(HeifConstants.QualityPresets.Professional, heif.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Slow, heif.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv444, heif.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.Equal(10, heif.BitDepth);
		Assert.Equal(HeifProfile.Main10, heif.Profile);
		Assert.True(heif.IsValid());

		// Verify professional metadata
		Assert.Equal("© Professional Photographer", heif.Metadata.Copyright);
		Assert.Equal("John Doe Photography", heif.Metadata.Author);
		Assert.Equal("Portrait photography with shallow depth of field", heif.Metadata.Description);

		// Verify professional camera metadata
		Assert.NotNull(heif.Metadata.CameraMetadata);
		Assert.Equal("Canon", heif.Metadata.CameraMetadata.CameraMake);
		Assert.Equal("EOS R5", heif.Metadata.CameraMetadata.CameraModel);
		Assert.Equal("Canon", heif.Metadata.CameraMetadata.LensMake);
		Assert.Equal("RF 85mm F1.2 L USM", heif.Metadata.CameraMetadata.LensModel);
		Assert.Equal(85.0, heif.Metadata.CameraMetadata.FocalLength);
		Assert.Equal(1.2, heif.Metadata.CameraMetadata.Aperture);
		Assert.Equal(1.0 / 200.0, heif.Metadata.CameraMetadata.ExposureTime);
		Assert.Equal(100, heif.Metadata.CameraMetadata.IsoSensitivity);
		Assert.Equal(300.0, heif.Metadata.CameraMetadata.XResolution);
		Assert.Equal(300.0, heif.Metadata.CameraMetadata.YResolution);
		Assert.Equal(2, heif.Metadata.CameraMetadata.ResolutionUnit); // Inches
		Assert.Equal(1, heif.Metadata.CameraMetadata.WhiteBalance); // Manual
		Assert.Equal(0, heif.Metadata.CameraMetadata.Flash); // Flash did not fire
		Assert.Equal(0.0, heif.Metadata.CameraMetadata.ExposureBias);
		Assert.Equal(3, heif.Metadata.CameraMetadata.MeteringMode); // Spot
		Assert.Equal(1, heif.Metadata.CameraMetadata.ExposureProgram); // Manual
		Assert.Equal(10, heif.Metadata.CameraMetadata.LightSource); // Flash
		Assert.Equal(85.0, heif.Metadata.CameraMetadata.FocalLengthIn35mm);
		Assert.Equal(1.0, heif.Metadata.CameraMetadata.DigitalZoomRatio);
		Assert.Equal(0, heif.Metadata.CameraMetadata.SceneCaptureType); // Standard
		Assert.Equal(0, heif.Metadata.CameraMetadata.Contrast); // Normal
		Assert.Equal(0, heif.Metadata.CameraMetadata.Saturation); // Normal
		Assert.Equal(1, heif.Metadata.CameraMetadata.Sharpness); // Soft
		Assert.Equal(2, heif.Metadata.CameraMetadata.SubjectDistanceRange); // Macro
		Assert.Equal(2, heif.Metadata.CameraMetadata.SensingMethod); // One-chip color area sensor
		Assert.Equal(0, heif.Metadata.CameraMetadata.GainControl); // None
		Assert.Equal("082024001234", heif.Metadata.CameraMetadata.BodySerialNumber);
		Assert.Equal("1234567890", heif.Metadata.CameraMetadata.LensSerialNumber);
		Assert.NotNull(heif.Metadata.CameraMetadata.LensSpecification);
		Assert.Equal(new[] { 85.0, 85.0, 1.2, 1.2 }, heif.Metadata.CameraMetadata.LensSpecification);
	}

	[Theory]
	[InlineData(320, 240)]
	[InlineData(640, 480)]
	[InlineData(1280, 720)]
	[InlineData(1920, 1080)]
	[InlineData(3840, 2160)]
	public void AllExamples_WithVariousDimensions_AreValid(int width, int height)
	{
		// Act & Assert
		Assert.True(HeifExamples.CreateWebOptimized(width, height).IsValid());
		Assert.True(HeifExamples.CreateHighQuality(width, height).IsValid());
		Assert.True(HeifExamples.CreateLossless(width, height).IsValid());
		Assert.True(HeifExamples.CreateFast(width, height).IsValid());
		Assert.True(HeifExamples.CreateHdr(width, height).IsValid());
		Assert.True(HeifExamples.CreateMobile(width, height).IsValid());
		Assert.True(HeifExamples.CreateAv1(width, height).IsValid());
		Assert.True(HeifExamples.CreateProfessionalPhoto(width, height).IsValid());
		Assert.True(HeifExamples.CreateDemo(width, height).IsValid());
	}

	[Fact]
	public void AllExamples_HaveValidEstimatedFileSizes()
	{
		// Arrange
		const int width = 1920;
		const int height = 1080;

		// Act
		var examples = new[]
		{
			HeifExamples.CreateWebOptimized(width, height),
			HeifExamples.CreateHighQuality(width, height),
			HeifExamples.CreateLossless(width, height),
			HeifExamples.CreateFast(width, height),
			HeifExamples.CreateHdr(width, height),
			HeifExamples.CreateMobile(width, height),
			HeifExamples.CreateThumbnail(256, 256),
			HeifExamples.CreateAv1(width, height),
			HeifExamples.CreateProfessionalPhoto(width, height),
			HeifExamples.CreateDemo(width, height)
		};

		// Assert
		foreach (var example in examples)
		{
			var fileSize = example.GetEstimatedFileSize();
			Assert.True(fileSize > 0, $"File size should be positive for {example.Metadata.Software}");
			Assert.True(fileSize < 100_000_000, $"File size should be reasonable for {example.Metadata.Software}");
		}
	}

	[Fact]
	public void AllExamples_HaveUniqueConfigurations()
	{
		// Arrange
		const int width = 1920;
		const int height = 1080;

		var examples = new[]
		{
			HeifExamples.CreateWebOptimized(width, height),
			HeifExamples.CreateHighQuality(width, height),
			HeifExamples.CreateLossless(width, height),
			HeifExamples.CreateFast(width, height),
			HeifExamples.CreateHdr(width, height),
			HeifExamples.CreateMobile(width, height),
			HeifExamples.CreateAv1(width, height)
		};

		// Assert - Each example should have different quality settings
		var qualitySettings = examples.Select(e => e.Quality).ToArray();
		Assert.Equal(qualitySettings.Length, qualitySettings.Distinct().Count());

		// Assert - Each example should have different speed settings
		var speedSettings = examples.Select(e => e.Speed).ToArray();
		Assert.Equal(speedSettings.Length, speedSettings.Distinct().Count());
	}

	[Fact]
	public void AllExamples_HaveNonNullMetadata()
	{
		// Arrange
		const int width = 1920;
		const int height = 1080;

		var examples = new[]
		{
			HeifExamples.CreateWebOptimized(width, height),
			HeifExamples.CreateHighQuality(width, height),
			HeifExamples.CreateLossless(width, height),
			HeifExamples.CreateFast(width, height),
			HeifExamples.CreateHdr(width, height),
			HeifExamples.CreateMobile(width, height),
			HeifExamples.CreateThumbnail(256, 256),
			HeifExamples.CreateAv1(width, height),
			HeifExamples.CreateProfessionalPhoto(width, height),
			HeifExamples.CreateDemo(width, height)
		};

		// Assert
		foreach (var example in examples)
		{
			Assert.NotNull(example.Metadata);
			Assert.NotNull(example.Metadata.Software);
			Assert.NotNull(example.Metadata.CreationTime);
			Assert.Contains("Wangkanai Graphics Library", example.Metadata.Software);
		}
	}
}
