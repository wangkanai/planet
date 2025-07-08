// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Provides factory methods and usage examples for creating AVIF raster images with common configurations.
/// </summary>
public static class AvifExamples
{
	/// <summary>Creates a standard quality AVIF image suitable for web use.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether to include alpha channel support.</param>
	/// <returns>A configured AVIF raster with web-optimized settings.</returns>
	public static AvifRaster CreateWebOptimized(int width, int height, bool hasAlpha = false)
	{
		var avif = new AvifRaster(width, height, hasAlpha)
		{
			Quality = AvifConstants.QualityPresets.Web,
			Speed = AvifConstants.SpeedPresets.Fast,
			ChromaSubsampling = AvifChromaSubsampling.Yuv420,
			ColorSpace = AvifColorSpace.Srgb,
			BitDepth = 8
		};

		return avif;
	}

	/// <summary>Creates a high-quality AVIF image for professional photography.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether to include alpha channel support.</param>
	/// <returns>A configured AVIF raster with professional quality settings.</returns>
	public static AvifRaster CreateProfessionalQuality(int width, int height, bool hasAlpha = false)
	{
		var avif = new AvifRaster(width, height, hasAlpha)
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Slow,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444,
			ColorSpace = AvifColorSpace.DisplayP3,
			BitDepth = 10
		};

		return avif;
	}

	/// <summary>Creates a lossless AVIF image for archival purposes.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="hasAlpha">Whether to include alpha channel support.</param>
	/// <returns>A configured AVIF raster with lossless compression.</returns>
	public static AvifRaster CreateLossless(int width, int height, bool hasAlpha = false)
	{
		var avif = new AvifRaster(width, height, hasAlpha)
		{
			IsLossless = true,
			Speed = AvifConstants.SpeedPresets.Slow,
			ColorSpace = AvifColorSpace.Srgb,
			BitDepth = hasAlpha ? 10 : 8
		};

		return avif;
	}

	/// <summary>Creates an HDR AVIF image with BT.2100 PQ color space.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="maxLuminance">Maximum display luminance in nits.</param>
	/// <param name="minLuminance">Minimum display luminance in nits.</param>
	/// <returns>A configured AVIF raster with HDR10 settings.</returns>
	public static AvifRaster CreateHdr10(int width, int height, double maxLuminance = 4000.0, double minLuminance = 0.01)
	{
		var avif = new AvifRaster(width, height, false)
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Default,
			ChromaSubsampling = AvifChromaSubsampling.Yuv422,
			ColorSpace = AvifColorSpace.Bt2100Pq,
			BitDepth = 10
		};

		// Configure HDR metadata
		var hdrMetadata = new HdrMetadata
		{
			Format = HdrFormat.Hdr10,
			MaxLuminance = maxLuminance,
			MinLuminance = minLuminance,
			MaxContentLightLevel = maxLuminance,
			MaxFrameAverageLightLevel = maxLuminance * 0.75
			// Note: Full implementation would include primary colors and white point
		};

		avif.SetHdrMetadata(hdrMetadata);
		return avif;
	}

	/// <summary>Creates an HDR AVIF image with BT.2100 HLG color space.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="gamma">System gamma value for HLG.</param>
	/// <returns>A configured AVIF raster with HLG HDR settings.</returns>
	public static AvifRaster CreateHlg(int width, int height, double gamma = 1.2)
	{
		var avif = new AvifRaster(width, height, false)
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Default,
			ChromaSubsampling = AvifChromaSubsampling.Yuv422,
			ColorSpace = AvifColorSpace.Bt2100Hlg,
			BitDepth = 10
		};

		// Configure HLG metadata
		var hdrMetadata = new HdrMetadata
		{
			Format = HdrFormat.Hlg,
			MaxLuminance = 1000.0,
			MinLuminance = 0.005
			// Note: SystemGamma and color primaries would be set in full implementation
		};

		avif.SetHdrMetadata(hdrMetadata);
		return avif;
	}

	/// <summary>Creates a fast-encoding AVIF image optimized for real-time applications.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <returns>A configured AVIF raster with fast encoding settings.</returns>
	public static AvifRaster CreateFastEncoding(int width, int height)
	{
		var avif = new AvifRaster(width, height, false)
		{
			Quality = AvifConstants.QualityPresets.Standard,
			Speed = AvifConstants.SpeedPresets.Fastest,
			ChromaSubsampling = AvifChromaSubsampling.Yuv420,
			ColorSpace = AvifColorSpace.Srgb,
			BitDepth = 8,
			ThreadCount = Environment.ProcessorCount
		};

		return avif;
	}

	/// <summary>Creates an AVIF image with film grain synthesis for natural texture.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="grainIntensity">Film grain intensity (0.0 to 1.0).</param>
	/// <returns>A configured AVIF raster with film grain enabled.</returns>
	public static AvifRaster CreateWithFilmGrain(int width, int height, float grainIntensity = 0.5f)
	{
		var avif = new AvifRaster(width, height, false)
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Default,
			ChromaSubsampling = AvifChromaSubsampling.Yuv422,
			ColorSpace = AvifColorSpace.Srgb,
			BitDepth = 8,
			EnableFilmGrain = true
		};

		// Note: Film grain intensity would be set in metadata in full implementation
		return avif;
	}

	/// <summary>Creates a thumbnail-optimized AVIF image.</summary>
	/// <param name="width">Thumbnail width in pixels (recommended: 150-300).</param>
	/// <param name="height">Thumbnail height in pixels (recommended: 150-300).</param>
	/// <returns>A configured AVIF raster optimized for thumbnails.</returns>
	public static AvifRaster CreateThumbnail(int width, int height)
	{
		if (width > 512 || height > 512)
			throw new ArgumentException("Thumbnail dimensions should not exceed 512x512 pixels for optimal performance.");

		var avif = new AvifRaster(width, height, false)
		{
			Quality = AvifConstants.QualityPresets.Thumbnail,
			Speed = AvifConstants.SpeedPresets.Fast,
			ChromaSubsampling = AvifChromaSubsampling.Yuv420,
			ColorSpace = AvifColorSpace.Srgb,
			BitDepth = 8
		};

		return avif;
	}

	/// <summary>Creates an AVIF image with wide color gamut support.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="colorSpace">Wide gamut color space to use.</param>
	/// <returns>A configured AVIF raster with wide color gamut settings.</returns>
	public static AvifRaster CreateWideGamut(int width, int height, AvifColorSpace colorSpace = AvifColorSpace.DisplayP3)
	{
		if (colorSpace != AvifColorSpace.DisplayP3 && colorSpace != AvifColorSpace.Bt2020Ncl)
			throw new ArgumentException("Color space must be DisplayP3 or BT2020 for wide gamut.");

		var avif = new AvifRaster(width, height, false)
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Default,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444,
			ColorSpace = colorSpace,
			BitDepth = 10
		};

		return avif;
	}

	/// <summary>Creates an AVIF image with alpha transparency support.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <param name="premultipliedAlpha">Whether to use premultiplied alpha.</param>
	/// <returns>A configured AVIF raster with alpha channel support.</returns>
	public static AvifRaster CreateWithAlpha(int width, int height, bool premultipliedAlpha = false)
	{
		var avif = new AvifRaster(width, height, true)
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Default,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444,
			ColorSpace = AvifColorSpace.Srgb,
			BitDepth = 8
		};

		avif.AvifMetadata.AlphaPremultiplied = premultipliedAlpha;
		return avif;
	}

	/// <summary>Creates an AVIF image optimized for 12-bit content.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <returns>A configured AVIF raster with 12-bit depth settings.</returns>
	public static AvifRaster CreateTwelveBit(int width, int height)
	{
		var avif = new AvifRaster(width, height, false)
		{
			Quality = AvifConstants.QualityPresets.Professional,
			Speed = AvifConstants.SpeedPresets.Slow,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444,
			ColorSpace = AvifColorSpace.Bt2020Ncl,
			BitDepth = 12
		};

		return avif;
	}

	/// <summary>Applies EXIF metadata to an AVIF image.</summary>
	/// <param name="avif">The AVIF raster to modify.</param>
	/// <param name="camera">Camera make and model.</param>
	/// <param name="lens">Lens information.</param>
	/// <param name="settings">Camera settings (ISO, aperture, shutter speed).</param>
	/// <param name="gpsLocation">GPS coordinates (latitude, longitude).</param>
	public static void ApplyExifMetadata(AvifRaster avif, string? camera = null, string? lens = null, 
		string? settings = null, (double Latitude, double Longitude)? gpsLocation = null)
	{
		var exifData = new List<byte>();
		
		// Simplified EXIF header
		exifData.AddRange(System.Text.Encoding.ASCII.GetBytes("Exif\0\0"));
		
		// Note: In a full implementation, camera make, model, lens, and GPS data
		// would be properly encoded into EXIF format and stored in metadata
		
		avif.AvifMetadata.ExifData = exifData.ToArray();
	}

	/// <summary>Demonstrates encoding an AVIF image with various quality presets.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <returns>Dictionary of quality preset names and their encoded results.</returns>
	public static async Task<Dictionary<string, byte[]>> DemonstrateQualityPresets(int width, int height)
	{
		var results = new Dictionary<string, byte[]>();

		// Test different quality presets
		var presets = new Dictionary<string, int>
		{
			["Thumbnail"] = AvifConstants.QualityPresets.Thumbnail,
			["Web"] = AvifConstants.QualityPresets.Web,
			["Standard"] = AvifConstants.QualityPresets.Standard,
			["Professional"] = AvifConstants.QualityPresets.Professional,
			["NearLossless"] = AvifConstants.QualityPresets.NearLossless,
			["Lossless"] = AvifConstants.QualityPresets.Lossless
		};

		foreach (var (name, quality) in presets)
		{
			using var avif = new AvifRaster(width, height, false)
			{
				Quality = quality,
				IsLossless = quality == AvifConstants.QualityPresets.Lossless
			};

			results[name] = await avif.EncodeAsync();
		}

		return results;
	}

	/// <summary>Demonstrates different chroma subsampling options.</summary>
	/// <param name="width">Image width in pixels.</param>
	/// <param name="height">Image height in pixels.</param>
	/// <returns>Dictionary of subsampling names and their encoded results.</returns>
	public static async Task<Dictionary<string, byte[]>> DemonstrateChromaSubsampling(int width, int height)
	{
		var results = new Dictionary<string, byte[]>();

		var subsamplings = new Dictionary<string, AvifChromaSubsampling>
		{
			["YUV 4:4:4"] = AvifChromaSubsampling.Yuv444,
			["YUV 4:2:2"] = AvifChromaSubsampling.Yuv422,
			["YUV 4:2:0"] = AvifChromaSubsampling.Yuv420,
			["Monochrome"] = AvifChromaSubsampling.Yuv400
		};

		foreach (var (name, subsampling) in subsamplings)
		{
			using var avif = new AvifRaster(width, height, false)
			{
				Quality = AvifConstants.QualityPresets.Standard,
				ChromaSubsampling = subsampling
			};

			results[name] = await avif.EncodeAsync();
		}

		return results;
	}

	/// <summary>Creates an AVIF encoding options preset for specific use cases.</summary>
	/// <param name="useCase">The intended use case.</param>
	/// <returns>Configured encoding options for the use case.</returns>
	public static AvifEncodingOptions CreatePresetFor(AvifUseCase useCase)
	{
		return useCase switch
		{
			AvifUseCase.WebOptimized => new AvifEncodingOptions
			{
				Quality = AvifConstants.QualityPresets.Standard,
				Speed = AvifConstants.SpeedPresets.Default,
				ChromaSubsampling = AvifChromaSubsampling.Yuv420,
				ThreadCount = Math.Min(4, Environment.ProcessorCount)
			},
			
			AvifUseCase.Photography => new AvifEncodingOptions
			{
				Quality = AvifConstants.QualityPresets.Professional,
				Speed = AvifConstants.SpeedPresets.Slow,
				ChromaSubsampling = AvifChromaSubsampling.Yuv444,
				ThreadCount = Environment.ProcessorCount
			},
			
			AvifUseCase.Archival => new AvifEncodingOptions
			{
				Quality = AvifConstants.QualityPresets.Lossless,
				Speed = AvifConstants.SpeedPresets.Slowest,
				IsLossless = true,
				ChromaSubsampling = AvifChromaSubsampling.Yuv444,
				ThreadCount = Environment.ProcessorCount
			},
			
			AvifUseCase.Thumbnail => new AvifEncodingOptions
			{
				Quality = AvifConstants.QualityPresets.Thumbnail,
				Speed = AvifConstants.SpeedPresets.Fast,
				ChromaSubsampling = AvifChromaSubsampling.Yuv420,
				ThreadCount = 2
			},
			
			AvifUseCase.RealTime => new AvifEncodingOptions
			{
				Quality = AvifConstants.QualityPresets.Web,
				Speed = AvifConstants.SpeedPresets.Fastest,
				ChromaSubsampling = AvifChromaSubsampling.Yuv420,
				ThreadCount = Math.Min(2, Environment.ProcessorCount)
			},
			
			_ => throw new ArgumentException($"Unknown use case: {useCase}")
		};
	}
}

/// <summary>Defines common use cases for AVIF encoding.</summary>
public enum AvifUseCase
{
	/// <summary>Optimized for web delivery with good quality/size balance.</summary>
	WebOptimized,
	
	/// <summary>High quality for professional photography.</summary>
	Photography,
	
	/// <summary>Lossless compression for archival storage.</summary>
	Archival,
	
	/// <summary>Small thumbnails with fast encoding.</summary>
	Thumbnail,
	
	/// <summary>Real-time encoding with minimal delay.</summary>
	RealTime
}