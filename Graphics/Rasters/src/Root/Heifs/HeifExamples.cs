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

		heif.Metadata.Software = "Wangkanai Graphics Library";
		heif.Metadata.CreationTime = DateTime.UtcNow;

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

		heif.Metadata.Software = "Wangkanai Graphics Library - Professional";
		heif.Metadata.CreationTime = DateTime.UtcNow;

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

		heif.Metadata.Software = "Wangkanai Graphics Library - Lossless";
		heif.Metadata.CreationTime = DateTime.UtcNow;

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

		heif.Metadata.Software = "Wangkanai Graphics Library - Fast";
		heif.Metadata.CreationTime = DateTime.UtcNow;

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

		heif.Metadata.Software = "Wangkanai Graphics Library - HDR";
		heif.Metadata.CreationTime = DateTime.UtcNow;
		heif.Metadata.ColorSpaceInfo = "BT.2100 PQ HDR";

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
			Speed = HeifConstants.SpeedPresets.Fast,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Hevc,
			BitDepth = 8,
			ThreadCount = Math.Min(Environment.ProcessorCount, 4),
			GenerateThumbnails = true,
			EnableProgressiveDecoding = true
		};

		heif.Metadata.Software = "Wangkanai Graphics Library - Mobile";
		heif.Metadata.CreationTime = DateTime.UtcNow;

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

		heif.Metadata.Software = "Wangkanai Graphics Library - Thumbnail";
		heif.Metadata.CreationTime = DateTime.UtcNow;

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
			Quality = HeifConstants.QualityPresets.High,
			Speed = HeifConstants.SpeedPresets.Medium,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			Compression = HeifCompression.Av1,
			BitDepth = 10,
			ThreadCount = 0, // Auto-detect
			GenerateThumbnails = true,
			EnableProgressiveDecoding = true
		};

		heif.Metadata.Software = "Wangkanai Graphics Library - AV1";
		heif.Metadata.CreationTime = DateTime.UtcNow;

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
		heif.Metadata.Software = "Wangkanai Graphics Library - Demo";
		heif.Metadata.CreationTime = DateTime.UtcNow;
		heif.Metadata.Description = "Demonstration HEIF image created with Wangkanai Graphics Library";
		heif.Metadata.Copyright = "© 2025 Wangkanai";
		heif.Metadata.Author = "Graphics Library";
		heif.Metadata.CameraMetadata = new CameraMetadata
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
			ResolutionUnit = 2 // Inches
		};
		heif.Metadata.ColorSpaceInfo = "sRGB";
		heif.Metadata.WhiteBalance = "Auto";

		// Add GPS coordinates (Sydney Opera House)
		heif.Metadata.GpsCoordinates = new GpsCoordinates
		{
			Latitude = -33.8568,
			Longitude = 151.2153,
			Altitude = 5.0
		};

		return heif;
	}
}