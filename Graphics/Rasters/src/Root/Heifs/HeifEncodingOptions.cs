// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Represents encoding options for HEIF images with comprehensive configuration settings.
/// </summary>
public sealed class HeifEncodingOptions
{
	/// <summary>
	/// Gets or sets the quality level for encoding (0-100).
	/// </summary>
	public int Quality { get; set; } = HeifConstants.DefaultQuality;

	/// <summary>
	/// Gets or sets the encoder speed setting (0-9, where 0 is slowest/best quality).
	/// </summary>
	public int Speed { get; set; } = HeifConstants.DefaultSpeed;

	/// <summary>
	/// Gets or sets whether to use lossless compression.
	/// </summary>
	public bool IsLossless { get; set; }

	/// <summary>
	/// Gets or sets the chroma subsampling mode.
	/// </summary>
	public HeifChromaSubsampling ChromaSubsampling { get; set; } = HeifChromaSubsampling.Yuv420;

	/// <summary>
	/// Gets or sets the number of threads to use for encoding (null = use default).
	/// </summary>
	public int? ThreadCount { get; set; }

	/// <summary>
	/// Gets or sets the compression method to use (null = use default).
	/// </summary>
	public HeifCompression? Compression { get; set; }

	/// <summary>
	/// Gets or sets the HEIF profile to use (null = use default).
	/// </summary>
	public HeifProfile? Profile { get; set; }

	/// <summary>
	/// Gets or sets whether to enable progressive decoding.
	/// </summary>
	public bool EnableProgressiveDecoding { get; set; }

	/// <summary>
	/// Gets or sets whether to generate thumbnails automatically.
	/// </summary>
	public bool GenerateThumbnails { get; set; } = true;

	/// <summary>
	/// Gets or sets the maximum memory usage in MB for pixel buffers.
	/// </summary>
	public int MaxPixelBufferSizeMB { get; set; } = HeifConstants.Memory.DefaultPixelBufferSizeMB;

	/// <summary>
	/// Gets or sets the maximum memory usage in MB for metadata buffers.
	/// </summary>
	public int MaxMetadataBufferSizeMB { get; set; } = HeifConstants.Memory.DefaultMetadataBufferSizeMB;

	/// <summary>
	/// Gets or sets the tile size for encoding large images.
	/// </summary>
	public int TileSize { get; set; } = HeifConstants.Memory.DefaultTileSize;

	/// <summary>
	/// Gets or sets whether to preserve metadata during encoding.
	/// </summary>
	public bool PreserveMetadata { get; set; } = true;

	/// <summary>
	/// Gets or sets whether to preserve color profiles.
	/// </summary>
	public bool PreserveColorProfile { get; set; } = true;

	/// <summary>
	/// Gets or sets codec-specific parameters.
	/// </summary>
	public Dictionary<string, object> CodecParameters { get; set; } = new();

	/// <summary>
	/// Gets or sets custom encoding parameters.
	/// </summary>
	public Dictionary<string, object> CustomParameters { get; set; } = new();

	/// <summary>
	/// Creates default encoding options.
	/// </summary>
	/// <returns>Default encoding options.</returns>
	public static HeifEncodingOptions CreateDefault()
	{
		return new HeifEncodingOptions();
	}

	/// <summary>
	/// Creates lossless encoding options.
	/// </summary>
	/// <returns>Lossless encoding options.</returns>
	public static HeifEncodingOptions CreateLossless()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.Lossless,
			Speed = HeifConstants.SpeedPresets.Slow,
			IsLossless = true,
			ChromaSubsampling = HeifChromaSubsampling.Yuv444
		};
	}

	/// <summary>
	/// Creates web-optimized encoding options.
	/// </summary>
	/// <returns>Web-optimized encoding options.</returns>
	public static HeifEncodingOptions CreateWebOptimized()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.Web,
			Speed = HeifConstants.SpeedPresets.Fast,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			GenerateThumbnails = true
		};
	}

	/// <summary>
	/// Creates high-quality encoding options.
	/// </summary>
	/// <returns>High-quality encoding options.</returns>
	public static HeifEncodingOptions CreateHighQuality()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.Professional,
			Speed = HeifConstants.SpeedPresets.Slow,
			ChromaSubsampling = HeifChromaSubsampling.Yuv444
		};
	}

	/// <summary>
	/// Creates fast encoding options.
	/// </summary>
	/// <returns>Fast encoding options.</returns>
	public static HeifEncodingOptions CreateFast()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.Standard,
			Speed = HeifConstants.SpeedPresets.Fastest,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420
		};
	}

	/// <summary>
	/// Creates HDR encoding options.
	/// </summary>
	/// <returns>HDR encoding options.</returns>
	public static HeifEncodingOptions CreateHdr()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.High,
			Speed = HeifConstants.SpeedPresets.Medium,
			ChromaSubsampling = HeifChromaSubsampling.Yuv444,
			Compression = HeifCompression.Hevc,
			Profile = HeifProfile.Main10
		};
	}

	/// <summary>
	/// Creates thumbnail encoding options.
	/// </summary>
	/// <returns>Thumbnail encoding options.</returns>
	public static HeifEncodingOptions CreateThumbnail()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.Thumbnail,
			Speed = HeifConstants.SpeedPresets.Fast,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			GenerateThumbnails = false
		};
	}

	/// <summary>
	/// Creates mobile-optimized encoding options.
	/// </summary>
	/// <returns>Mobile-optimized encoding options.</returns>
	public static HeifEncodingOptions CreateMobile()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.Mobile,
			Speed = HeifConstants.SpeedPresets.Fast,
			ChromaSubsampling = HeifChromaSubsampling.Yuv420,
			MaxPixelBufferSizeMB = 256,
			TileSize = 512
		};
	}

	/// <summary>
	/// Creates archival quality encoding options.
	/// </summary>
	/// <returns>Archival quality encoding options.</returns>
	public static HeifEncodingOptions CreateArchival()
	{
		return new HeifEncodingOptions
		{
			Quality = HeifConstants.QualityPresets.NearLossless,
			Speed = HeifConstants.SpeedPresets.Slowest,
			ChromaSubsampling = HeifChromaSubsampling.Yuv444,
			PreserveMetadata = true,
			PreserveColorProfile = true
		};
	}

	/// <summary>
	/// Validates the encoding options.
	/// </summary>
	/// <param name="error">Error message if validation fails.</param>
	/// <returns>True if valid, false otherwise.</returns>
	public bool Validate(out string? error)
	{
		if (Quality < HeifConstants.MinQuality || Quality > HeifConstants.MaxQuality)
		{
			error = $"Quality must be between {HeifConstants.MinQuality} and {HeifConstants.MaxQuality}.";
			return false;
		}

		if (Speed < HeifConstants.MinSpeed || Speed > HeifConstants.MaxSpeed)
		{
			error = $"Speed must be between {HeifConstants.MinSpeed} and {HeifConstants.MaxSpeed}.";
			return false;
		}

		if (IsLossless && Quality != HeifConstants.QualityPresets.Lossless)
		{
			error = "Lossless mode requires quality to be 100.";
			return false;
		}

		if (ThreadCount.HasValue && ThreadCount.Value < 0)
		{
			error = "Thread count cannot be negative.";
			return false;
		}

		if (ThreadCount.HasValue && ThreadCount.Value > HeifConstants.Memory.MaxThreads)
		{
			error = $"Thread count cannot exceed {HeifConstants.Memory.MaxThreads}.";
			return false;
		}

		if (MaxPixelBufferSizeMB <= 0 || MaxPixelBufferSizeMB > HeifConstants.Memory.MaxPixelBufferSizeMB)
		{
			error = $"Pixel buffer size must be between 1 and {HeifConstants.Memory.MaxPixelBufferSizeMB} MB.";
			return false;
		}

		if (MaxMetadataBufferSizeMB <= 0 || MaxMetadataBufferSizeMB > HeifConstants.Memory.MaxMetadataBufferSizeMB)
		{
			error = $"Metadata buffer size must be between 1 and {HeifConstants.Memory.MaxMetadataBufferSizeMB} MB.";
			return false;
		}

		if (TileSize <= 0 || TileSize > 8192)
		{
			error = "Tile size must be between 1 and 8192 pixels.";
			return false;
		}

		// Validate codec-specific constraints
		if (Compression == HeifCompression.Jpeg && IsLossless)
		{
			error = "JPEG compression cannot be used with lossless mode.";
			return false;
		}

		if (Profile == HeifProfile.Main10 && ChromaSubsampling == HeifChromaSubsampling.Yuv400)
		{
			error = "Main 10 profile cannot be used with monochrome (YUV 4:0:0) subsampling.";
			return false;
		}

		error = null;
		return true;
	}

	/// <summary>
	/// Creates a copy of these encoding options.
	/// </summary>
	/// <returns>A new encoding options instance with copied values.</returns>
	public HeifEncodingOptions Clone()
	{
		return new HeifEncodingOptions
		{
			Quality = Quality,
			Speed = Speed,
			IsLossless = IsLossless,
			ChromaSubsampling = ChromaSubsampling,
			ThreadCount = ThreadCount,
			Compression = Compression,
			Profile = Profile,
			EnableProgressiveDecoding = EnableProgressiveDecoding,
			GenerateThumbnails = GenerateThumbnails,
			MaxPixelBufferSizeMB = MaxPixelBufferSizeMB,
			MaxMetadataBufferSizeMB = MaxMetadataBufferSizeMB,
			TileSize = TileSize,
			PreserveMetadata = PreserveMetadata,
			PreserveColorProfile = PreserveColorProfile,
			CodecParameters = new Dictionary<string, object>(CodecParameters),
			CustomParameters = new Dictionary<string, object>(CustomParameters)
		};
	}
}