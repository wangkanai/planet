// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

using Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Provides factory methods and examples for creating HEIF raster images with common configurations.
/// </summary>
public static class HeifExamples
{
	/// <summary>
	/// Creates a web-optimized HEIF image with balanced quality and file size.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>A HEIF raster configured for web use.</returns>
	public static HeifRaster CreateWebOptimized(int width, int height, bool hasAlpha = false)
	{
		var heif = new HeifRaster(width, height, hasAlpha)
		{
			Quality = HeifConstants.QualityPresets.Web,
			Speed = HeifConstants.SpeedPresets.Fast,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Hevc,
			BitDepth = 8,
			ThreadCount = 0, // Auto-detect
			GenerateThumbnails = true,
			EnableProgressiveDecoding = true
		};

		heif.HeifMetadata.Software = "Wangkanai Graphics Library";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;

		return heif;
	}

	/// <summary>
	/// Creates a high-quality HEIF image for photography.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>A HEIF raster configured for high-quality photography.</returns>
	public static HeifRaster CreateHighQuality(int width, int height, bool hasAlpha = false)
	{
		var heif = new HeifRaster(width, height, hasAlpha)
		{
			Quality = HeifConstants.QualityPresets.Professional,
			Speed = HeifConstants.SpeedPresets.Slow,
			ChromaSubsampling = HeifChromaSubsampling.Yuv444,
			Compression = HeifCompression.Hevc,
			BitDepth = 10,
			Profile = HeifProfile.Main10,
			ThreadCount = 0, // Auto-detect
			GenerateThumbnails = true,
			EnableProgressiveDecoding = false
		};

		heif.HeifMetadata.Software = "Wangkanai Graphics Library - Professional";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;

		return heif;
	}

	/// <summary>
	/// Creates a lossless HEIF image for archival purposes.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>A HEIF raster configured for lossless compression.</returns>
	public static HeifRaster CreateLossless(int width, int height, bool hasAlpha = false)
	{
		var heif = new HeifRaster(width, height, hasAlpha)
		{
			Quality = HeifConstants.QualityPresets.Lossless,
			Speed = HeifConstants.SpeedPresets.Slowest,
			ChromaSubsampling = HeifChromaSubsampling.Yuv444,
			Compression = HeifCompression.Hevc,
			BitDepth = 12,
			Profile = HeifProfile.Main10,
			IsLossless = true,
			ThreadCount = 0, // Auto-detect
			GenerateThumbnails = true,
			EnableProgressiveDecoding = false
		};

		heif.HeifMetadata.Software = "Wangkanai Graphics Library - Lossless";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;

		return heif;
	}

	/// <summary>
	/// Creates a fast-encoding HEIF image for real-time applications.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>A HEIF raster configured for fast encoding.</returns>
	public static HeifRaster CreateFast(int width, int height, bool hasAlpha = false)
	{
		var heif = new HeifRaster(width, height, hasAlpha)
		{
			Quality = HeifConstants.QualityPresets.Standard,
			Speed = HeifConstants.SpeedPresets.Fastest,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Hevc,
			BitDepth = 8,
			ThreadCount = Environment.ProcessorCount,
			GenerateThumbnails = false,
			EnableProgressiveDecoding = false
		};

		heif.HeifMetadata.Software = "Wangkanai Graphics Library - Fast";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;

		return heif;
	}

	/// <summary>
	/// Creates an HDR HEIF image with advanced color capabilities.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>A HEIF raster configured for HDR content.</returns>
	public static HeifRaster CreateHdr(int width, int height, bool hasAlpha = false)
	{
		var heif = new HeifRaster(width, height, hasAlpha)
		{
			Quality = HeifConstants.QualityPresets.High,
			Speed = HeifConstants.SpeedPresets.Medium,
			ChromaSubsampling = HeifChromaSubsampling.Yuv444,
			Compression = HeifCompression.Hevc,
			ColorSpace = HeifColorSpace.Bt2100Pq,
			BitDepth = 10,
			Profile = HeifProfile.Main10,
			ThreadCount = 0, // Auto-detect
			GenerateThumbnails = true,
			EnableProgressiveDecoding = true
		};

		// Set HDR metadata
		heif.SetHdrMetadata(new HdrMetadata
		{
			MaxLuminance = HeifConstants.Hdr.Hdr10PeakBrightness,
			MinLuminance = 0.005,
			MaxContentLightLevel = 1000,
			MaxFrameAverageLightLevel = 400,
			ColorPrimaries = HdrColorPrimaries.Bt2020,
			TransferCharacteristics = HdrTransferCharacteristics.Pq,
			MatrixCoefficients = HdrMatrixCoefficients.Bt2020Ncl
		});

		heif.HeifMetadata.Software = "Wangkanai Graphics Library - HDR";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;
		heif.HeifMetadata.ColorSpaceInfo = "BT.2100 PQ HDR";

		return heif;
	}

	/// <summary>
	/// Creates a mobile-optimized HEIF image with reduced memory usage.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>A HEIF raster configured for mobile devices.</returns>
	public static HeifRaster CreateMobile(int width, int height, bool hasAlpha = false)
	{
		var heif = new HeifRaster(width, height, hasAlpha)
		{
			Quality = HeifConstants.QualityPresets.Mobile,
			Speed = HeifConstants.SpeedPresets.MediumFast,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Hevc,
			BitDepth = 8,
			ThreadCount = Math.Min(Environment.ProcessorCount, 4),
			GenerateThumbnails = true,
			EnableProgressiveDecoding = true
		};

		heif.HeifMetadata.Software = "Wangkanai Graphics Library - Mobile";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;

		return heif;
	}

	/// <summary>
	/// Creates a thumbnail HEIF image with high compression.
	/// </summary>
	/// <param name="maxWidth">Maximum width in pixels.</param>
	/// <param name="maxHeight">Maximum height in pixels.</param>
	/// <returns>A HEIF raster configured for thumbnail generation.</returns>
	public static HeifRaster CreateThumbnail(int maxWidth, int maxHeight)
	{
		var width = Math.Min(maxWidth, 512);
		var height = Math.Min(maxHeight, 512);

		var heif = new HeifRaster(width, height, false)
		{
			Quality = HeifConstants.QualityPresets.Thumbnail,
			Speed = HeifConstants.SpeedPresets.Fastest,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Hevc,
			BitDepth = 8,
			ThreadCount = 1,
			GenerateThumbnails = false,
			EnableProgressiveDecoding = false
		};

		heif.HeifMetadata.Software = "Wangkanai Graphics Library - Thumbnail";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;

		return heif;
	}

	/// <summary>
	/// Creates a HEIF image with AV1 compression for modern compatibility.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether the image has an alpha channel.</param>
	/// <returns>A HEIF raster configured with AV1 compression.</returns>
	public static HeifRaster CreateAv1(int width, int height, bool hasAlpha = false)
	{
		var heif = new HeifRaster(width, height, hasAlpha)
		{
			Quality = HeifConstants.QualityPresets.NearLossless,
			Speed = HeifConstants.SpeedPresets.MediumSlow,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Av1,
			BitDepth = 10,
			ThreadCount = 0, // Auto-detect
			GenerateThumbnails = true,
			EnableProgressiveDecoding = true
		};

		heif.HeifMetadata.Software = "Wangkanai Graphics Library - AV1";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;

		return heif;
	}

	/// <summary>
	/// Creates a professional photography HEIF image with comprehensive camera metadata.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <returns>A HEIF raster configured for professional photography with full metadata.</returns>
	public static HeifRaster CreateProfessionalPhoto(int width, int height)
	{
		var heif = CreateHighQuality(width, height, false);

		// Professional camera metadata
		heif.HeifMetadata.CameraMetadata = new CameraMetadata
		{
			CameraMake = "Canon",
			CameraModel = "EOS R5",
			LensMake = "Canon",
			LensModel = "RF 85mm F1.2 L USM",
			FocalLength = 85.0,
			Aperture = 1.2,
			ExposureTime = 1.0 / 200.0,
			IsoSensitivity = 100,
			XResolution = 300.0,
			YResolution = 300.0,
			ResolutionUnit = 2, // Inches
			WhiteBalance = 1, // Manual
			Flash = 0, // Flash did not fire
			ExposureBias = 0.0,
			MeteringMode = 3, // Spot
			ExposureProgram = 1, // Manual
			LightSource = 10, // Flash
			FocalLengthIn35mm = 85.0,
			DigitalZoomRatio = 1.0,
			SceneCaptureType = 0, // Standard
			Contrast = 0, // Normal
			Saturation = 0, // Normal
			Sharpness = 1, // Soft
			SubjectDistanceRange = 2, // Macro
			SensingMethod = 2, // One-chip color area sensor
			GainControl = 0, // None
			BodySerialNumber = "082024001234",
			LensSerialNumber = "1234567890",
			LensSpecification = new[] { 85.0, 85.0, 1.2, 1.2 } // 85mm f/1.2
		};

		heif.HeifMetadata.Copyright = "© Professional Photographer";
		heif.HeifMetadata.Author = "John Doe Photography";
		heif.HeifMetadata.Description = "Portrait photography with shallow depth of field";

		return heif;
	}

	/// <summary>
	/// Creates a demonstration HEIF image with comprehensive metadata.
	/// </summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <returns>A HEIF raster with example metadata for demonstration purposes.</returns>
	public static HeifRaster CreateDemo(int width = 1920, int height = 1080)
	{
		var heif = new HeifRaster(width, height, false)
		{
			Quality = HeifConstants.QualityPresets.High,
			Speed = HeifConstants.SpeedPresets.Default,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Hevc,
			BitDepth = 8,
			ThreadCount = 0,
			GenerateThumbnails = true,
			EnableProgressiveDecoding = true
		};

		// Add comprehensive demo metadata
		heif.HeifMetadata.Software = "Wangkanai Graphics Library - Demo";
		heif.HeifMetadata.CreationTime = DateTime.UtcNow;
		heif.HeifMetadata.Description = "Demonstration HEIF image created with Wangkanai Graphics Library";
		heif.HeifMetadata.Copyright = "© 2025 Wangkanai";
		heif.HeifMetadata.Author = "Graphics Library";
		heif.HeifMetadata.CameraMetadata = new CameraMetadata
		{
			CameraMake = "Demo Camera",
			CameraModel = "Model X",
			LensMake = "Demo Lens",
			LensModel = "50mm f/1.8",
			FocalLength = 50.0,
			Aperture = 1.8,
			ExposureTime = 1.0 / 60.0,
			IsoSensitivity = 100,
			XResolution = 300.0,
			YResolution = 300.0,
			ResolutionUnit = 2, // Inches
			WhiteBalance = 0, // Auto
			Flash = 16, // Flash fired, auto mode
			ExposureBias = 0.0,
			MeteringMode = 5, // Pattern
			ExposureProgram = 2, // Normal program
			LightSource = 0, // Unknown
			FocalLengthIn35mm = 75.0,
			DigitalZoomRatio = 1.0,
			SceneCaptureType = 0, // Standard
			Contrast = 0, // Normal
			Saturation = 0, // Normal
			Sharpness = 0, // Normal
			SubjectDistanceRange = 0, // Unknown
			SensingMethod = 2, // One-chip color area sensor
			GainControl = 0, // None
			BodySerialNumber = "DEMO12345",
			LensSerialNumber = "LENS67890",
			LensSpecification = new[] { 24.0, 70.0, 2.8, 2.8 }, // 24-70mm f/2.8
			// GPS data can also be stored in CameraMetadata
			GpsLatitude = -33.8568,
			GpsLongitude = 151.2153,
			GpsAltitude = 5.0,
			GpsTimestamp = DateTime.UtcNow
		};
		heif.HeifMetadata.ColorSpaceInfo = "sRGB";
		heif.HeifMetadata.WhiteBalance = "Auto";

		// GPS coordinates can also be stored separately in HeifMetadata
		// This provides a more detailed GPS structure with timestamp
		heif.HeifMetadata.GpsCoordinates = new GpsCoordinates
		{
			Latitude = -33.8568,
			Longitude = 151.2153,
			Altitude = 5.0,
			Timestamp = DateTimeOffset.UtcNow
		};

		return heif;
	}
}